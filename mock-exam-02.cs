// =============================================================================
// MOCK REFACTORING EXAM #02 — C# .NET
// Time limit: 60 minutes
// Task: Identify all code smells, then refactor the code below.
// Rules: preserve all existing behaviour, use .NET 8 idioms, no new NuGet packages.
// =============================================================================
//
// SMELLS TO FIND (fill in before you start coding):
//   1. Validate to Validate-Class.Validate-Method_______________________________________________
//   2. if/switch InvoiceType--> splite to new_class:interface
//   3. sql-injection to parameter_______________________________________________
//   4. duplicate db block-->single save()_______________________________________________
//   5. _______________________________________________
//   6. _______________________________________________
//
// YOUR REFACTORED CODE GOES BELOW THE DASHED LINE AT THE BOTTOM.
// =============================================================================

using System.Data.Common;
using System.Data.SqlClient;
using System.Net.Mail;

namespace MessyApp;

public class InvoiceManager
{
    public string Process(int id, string type, decimal amt, string cust, string email)
    {
        // validate
        if (amt <= 0)
            throw new Exception("bad amount");
        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            throw new Exception("bad email");
        if (string.IsNullOrEmpty(cust))
            throw new Exception("bad customer");

        decimal final = 0;
        string statusMsg = "";

        if (type == "standard")                     // Loop Type
        {
            if (amt > 10000)                        // Calculate
                final = amt - (amt * 0.05m);        //
            else                                    //
                final = amt;                        //
            
            // Invoices Database --> to Injection
            var conn = new SqlConnection("Server=localhost;Database=AppDb;User Id=sa;Password=Admin123!;");
            conn.Open();
            var cmd = new SqlCommand($"INSERT INTO Invoices (CustomerId, Amount, Type, Status) VALUES ({id}, {final}, 'standard', 'pending')", conn);
            cmd.ExecuteNonQuery();
            conn.Close();

            // Return StatusMessage
            statusMsg = $"Standard invoice created for {cust}. Amount: {final:C}";
        }
        else if (type == "recurring")
        {
            final = amt * 0.90m;

            var conn = new SqlConnection("Server=localhost;Database=AppDb;User Id=sa;Password=Admin123!;");
            conn.Open();
            var cmd = new SqlCommand($"INSERT INTO Invoices (CustomerId, Amount, Type, Status) VALUES ({id}, {final}, 'recurring', 'pending')", conn);
            cmd.ExecuteNonQuery();
            conn.Close();

            statusMsg = $"Recurring invoice created for {cust}. Amount: {final:C} (10% loyalty discount applied)";
        }
        else if (type == "credit")
        {
            if (amt > 50000)
                throw new Exception("credit limit exceeded");

            final = amt * -1;

            var conn = new SqlConnection("Server=localhost;Database=AppDb;User Id=sa;Password=Admin123!;");
            conn.Open();
            var cmd = new SqlCommand($"INSERT INTO Invoices (CustomerId, Amount, Type, Status) VALUES ({id}, {final}, 'credit', 'issued')", conn);
            cmd.ExecuteNonQuery();
            conn.Close();

            statusMsg = $"Credit note issued for {cust}. Amount: {final:C}";
        }
        else
        {
            throw new Exception("unknown type");
        }

        // send email
        var client = new SmtpClient("smtp.company.com");
        client.Port = 587;
        var msg = new MailMessage();
        msg.From = new MailAddress("billing@company.com");
        msg.To.Add(email);
        msg.Subject = "Invoice Notification";
        msg.Body = statusMsg;
        client.Send(msg);

        Console.WriteLine("invoice processed: " + id);

        return statusMsg;
    }
}

// REFACTORED

//using System.Data.SqlClient;
//using System.Net.Mail;

namespace MessyApp;

public enum EnumInvoiceType{Standard,  Recurring, Credit};

public record Invoice(int CustomerId,decimal Amount, EnumInvoiceType Type,string Status);
public record InvoiceResult(string Message, decimal FinalAmount );

public class InvoiceService
{
    private readonly IInvoiceRepository _repository;
    private readonly IMailer _mailer;


    public InvoiceService(IInvoiceRepository repository,IMailer mailer)
    {
        _repository = repository;
        _mailer = mailer;
    }

    public InvoiceResult Process(int InvoiceDd, EnumInvoiceType InvoiceType, 
        decimal amount, string customer, string email)
    {
        //
        Validation.Validate(amount, customer, email);

        // next todo

        //Cal
        var finalAmount = InvoiceCalculation.Calculate(amount, InvoiceType);
        // db || sendmail
        var status = InvoiceType == EnumInvoiceType.Credit ? "issued" : "pending";
        var invoice = new Invoice(InvoiceDd,finalAmount,InvoiceType,status); 
        await _repository.SaveAsync(invoice);

        var emailmessage = BuildMessage(InvoiceType,customer, finalAmount);

        var mail = new MailMessage("billing@company.com", order.CustomerEmail, 
            "Invoice Notification",
            emailmessage);
        await _mailer.SendAsync(mail);

        // return Msg
        return new InvoiceResult(emailmessage, finalAmount);
    }

    private static string BuildMessage(EnumInvoiceType type, string customerName, decimal amount)
    => type switch
    {

        EnumInvoiceType.Standard => $"Standard invoice created for {customerName}. Amount: {amount:C}",
        EnumInvoiceType.Credit => $"Credit note issued for {customerName}. Amount: {amount:C}",
        EnumInvoiceType.Recurring => $"Recurring invoice created for {customerName}. Amount: {amount:C} (10% loyalty discount applied)",
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };

}


// Validate -----------------
public static class Validation
{
    public static void Validate (decimal amount, string customer, 
        string email )
    {

        if (amount <= 0)
            return throw new ArgumentException("Amount must be positive", nameof(amount)); 
        if (string.IsNullOrWhiteSpace(customer))
            return throw new ArgumentException("Customer are require", nameof(customer));
        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            return throw new ArgumentException("Invalid email are require", nameof(email));


    }
}

public static class  InvoiceCalculation{
    private const decimal StandardBulkThreshold = 10_000m;
    private const decimal StandardBulkDiscount = 0.05m;
    private const decimal RecurringBulkDiscount = 0.90m;
    private const decimal CreditBulkThreshold = 50_000m;

    public static decimal Calculate(decimal amount, EnumInvoiceType InvoiceType)
    =>
        InvoiceType switch
        {
            EnumInvoiceType.Standard => amount > StandardBulkThreshold ? amount - (amount * StandardBulkDiscount) :  amount,
            EnumInvoiceType.Standard => amount * RecurringBulkDiscount,
            EnumInvoiceType.Standard => amount > CreditBulkThreshold ? 
                throw new InvalidOperationException("credit limit exceeded")
                : -amount,
            _ => throw new ArgumentOutOfRangeException(nameof(InvoiceType))

        };

}

// ── Infrastructure (adapters) ─────────────────────────────────────────────────
public interface IInvoiceRepository{
    Task SaveAsync(Invoice invoice);
}

public class SqlInvoiceRepository : IInvoiceRepository
{
    private readonly string _connectionString; 

    public SqlInvoiceRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task SaveAsync(Invoice invoice)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.ExecuteAsync(
            "INSERT INTO Invoices (CustomerId, Amount, Type, Status) VALUES (@CustomerId, @Amount, @Type, @Status)",
            new { invoice.CustomerId, invoice.Amount, invoice.Type, invoice.Status });
    }
}

//--------------------------

public interface IMailer
{
    Task SendAsync(MailMessage message);
}

// 2. settings model — from appsettings.json
public class MailSettings
{
    public string Host     { get; set; } = "smtp.company.com";
    public int    Port     { get; set; } = 587;
    //public string FromEmail { get; set; } = "billing@company.com";

}

// 3. concrete implementation
public class SmtpMailer : IMailer
{
    private readonly MailSettings _settings;
    public SmtpMailer(IOptions<MailSettings> options) => _settings = options.Value;

    public async Task SendAsync(MailMessage message)
    {
        using var smtp = new SmtpClient(_settings.Host)
        {
            Port = _settings.Port,
            Credentials = new NetworkCredential(_settings.UserName, _settings.Password),
            EnableSsl = true
        };
        await smtp.SendMailAsync(message);
    }
}