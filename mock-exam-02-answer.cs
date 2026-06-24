// =============================================================================
// MOCK EXAM #02 — ANSWER KEY
// Smells fixed:
//   1. SQL injection        → parameterized queries
//   2. Tight coupling       → IInvoiceRepository + IEmailSender injected
//   3. SRP violation        → validation / pricing / persistence / notification split
//   4. Magic literals       → constants + configuration
//   5. Duplicated DB block  → single SaveInvoice() call
//   6. Untestable design    → all I/O behind interfaces
// =============================================================================

using System.Net.Mail;
using System.Data.SqlClient;

namespace CleanApp;

// ── Domain ────────────────────────────────────────────────────────────────────

public enum InvoiceType { Standard, Recurring, Credit }

public record Invoice(int CustomerId, decimal Amount, InvoiceType Type, string Status);

public record InvoiceResult(string Message, decimal FinalAmount);

// ── Port interfaces (Application layer) ───────────────────────────────────────

public interface IInvoiceRepository
{
    void Save(Invoice invoice);
}

public interface IEmailSender
{
    void Send(string to, string subject, string body);
}

// ── Pricing (single responsibility) ───────────────────────────────────────────

public static class InvoicePricing
{
    private const decimal StandardBulkThreshold = 10_000m;
    private const decimal StandardBulkDiscount  = 0.05m;
    private const decimal RecurringDiscount      = 0.10m;
    private const decimal CreditLimit            = 50_000m;

    public static decimal Calculate(decimal amount, InvoiceType type)
    {
        return type switch
        {
            InvoiceType.Standard  => amount > StandardBulkThreshold
                                        ? amount * (1 - StandardBulkDiscount)
                                        : amount,
            InvoiceType.Recurring => amount * (1 - RecurringDiscount),
            InvoiceType.Credit    => amount > CreditLimit
                                        ? throw new InvalidOperationException("Credit limit exceeded")
                                        : -amount,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }
}

// ── Validation ────────────────────────────────────────────────────────────────

public static class InvoiceValidator
{
    public static void Validate(decimal amount, string customerName, string email)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive.", nameof(amount));
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name is required.", nameof(customerName));
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("A valid email is required.", nameof(email));
    }
}

// ── Application service ───────────────────────────────────────────────────────

public class InvoiceService
{
    private readonly IInvoiceRepository _repository;
    private readonly IEmailSender       _emailSender;

    public InvoiceService(IInvoiceRepository repository, IEmailSender emailSender)
    {
        _repository  = repository;
        _emailSender = emailSender;
    }

    public InvoiceResult Process(int customerId, InvoiceType type, decimal amount,
                                 string customerName, string email)
    {
        InvoiceValidator.Validate(amount, customerName, email);

        var finalAmount = InvoicePricing.Calculate(amount, type);
        var status      = type == InvoiceType.Credit ? "issued" : "pending";
        var invoice     = new Invoice(customerId, finalAmount, type, status);

        _repository.Save(invoice);

        var message = BuildMessage(type, customerName, finalAmount);
        _emailSender.Send(email, "Invoice Notification", message);

        return new InvoiceResult(message, finalAmount);
    }

    private static string BuildMessage(InvoiceType type, string customerName, decimal amount) =>
        type switch
        {
            InvoiceType.Standard  => $"Standard invoice created for {customerName}. Amount: {amount:C}",
            InvoiceType.Recurring => $"Recurring invoice created for {customerName}. Amount: {amount:C} (10% loyalty discount applied)",
            InvoiceType.Credit    => $"Credit note issued for {customerName}. Amount: {amount:C}",
            _                     => throw new ArgumentOutOfRangeException(nameof(type))
        };
}

// ── Infrastructure (adapters) ─────────────────────────────────────────────────

public class SqlInvoiceRepository : IInvoiceRepository
{
    private readonly string _connectionString;

    public SqlInvoiceRepository(string connectionString)
        => _connectionString = connectionString;

    public void Save(Invoice invoice)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        using var cmd = new SqlCommand(
            "INSERT INTO Invoices (CustomerId, Amount, Type, Status) VALUES (@cid, @amt, @type, @status)",
            conn);

        cmd.Parameters.AddWithValue("@cid",    invoice.CustomerId);
        cmd.Parameters.AddWithValue("@amt",    invoice.Amount);
        cmd.Parameters.AddWithValue("@type",   invoice.Type.ToString().ToLowerInvariant());
        cmd.Parameters.AddWithValue("@status", invoice.Status);

        cmd.ExecuteNonQuery();
    }
}

public class SmtpEmailSender : IEmailSender
{
    private const string FromAddress = "billing@company.com";
    private const string SmtpHost    = "smtp.company.com";
    private const int    SmtpPort    = 587;

    public void Send(string to, string subject, string body)
    {
        using var client = new SmtpClient(SmtpHost) { Port = SmtpPort };
        using var msg    = new MailMessage(FromAddress, to, subject, body);
        client.Send(msg);
    }
}

// ── Composition root (Program.cs or DI registration) ─────────────────────────
//
// builder.Services.AddScoped<IInvoiceRepository>(_ =>
//     new SqlInvoiceRepository(builder.Configuration.GetConnectionString("AppDb")!));
// builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
// builder.Services.AddScoped<InvoiceService>();
