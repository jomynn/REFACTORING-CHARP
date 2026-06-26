// =============================================================================
// MOCK EXAM #03 — ANSWER KEY
// Smells fixed:
//   1. SQL injection          → parameterized queries in both repositories
//   2. Hardcoded credentials  → connection string injected; not in source code
//   3. Swallowed exception    → payment errors propagate; no silent catch-all
//   4. Tight coupling         → HttpClient injected; await instead of .Result/.Wait()
//   5. DRY violation          → single DiscountCalculator.GetRate() covers all categories
//   6. SRP violation          → validate / lookup / discount / charge / persist / notify split
// =============================================================================

using System.Data.SqlClient;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace CleanShop;

// ── Domain ────────────────────────────────────────────────────────────────────

public enum OrderStatus { Paid, Pending }

public record Product(string Code, decimal UnitPrice, string Category);

public record Order(
    int OrderId, string CustomerId, string ProductCode,
    int Quantity, decimal Total, OrderStatus Status);

public record OrderSummary(int OrderId, decimal Total, OrderStatus Status);

// ── Port interfaces (Application layer) ───────────────────────────────────────

public interface IProductRepository
{
    Task<Product?> GetByCodeAsync(string code);
}

public interface IOrderRepository
{
    Task SaveAsync(Order order);
}

public interface IPaymentGateway
{
    Task<bool> ChargeAsync(string customerId, decimal amount, int orderId);
}

public interface INotificationService
{
    Task NotifyAsync(string customerId, string message);
}

// ── Discount calculator (pure; no I/O) ────────────────────────────────────────

public static class DiscountCalculator
{
    private const int    BulkLargeQty  = 10;
    private const int    BulkSmallQty  = 5;
    private const decimal BulkLargeRate = 0.15m;
    private const decimal BulkSmallRate = 0.08m;
    private const decimal BooksRate     = 0.05m;

    private static readonly HashSet<string> BulkEligible =
        new(StringComparer.OrdinalIgnoreCase) { "electronics", "clothing" };

    public static decimal GetRate(string category, int quantity)
    {
        if (BulkEligible.Contains(category))
        {
            if (quantity >= BulkLargeQty) return BulkLargeRate;
            if (quantity >= BulkSmallQty) return BulkSmallRate;
            return 0m;
        }
        return string.Equals(category, "books", StringComparison.OrdinalIgnoreCase)
            ? BooksRate
            : 0m;
    }
}

// ── Validation ────────────────────────────────────────────────────────────────

public static class OrderValidator
{
    public static void Validate(int orderId, string customerId, string productCode, int qty)
    {
        if (orderId <= 0)
            throw new ArgumentException("Order ID must be positive.", nameof(orderId));
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Customer ID is required.", nameof(customerId));
        if (string.IsNullOrWhiteSpace(productCode))
            throw new ArgumentException("Product code is required.", nameof(productCode));
        if (qty <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(qty));
    }
}

// ── Application service ───────────────────────────────────────────────────────

public class OrderService
{
    private readonly IProductRepository  _products;
    private readonly IOrderRepository    _orders;
    private readonly IPaymentGateway     _payments;
    private readonly INotificationService _notifications;

    public OrderService(
        IProductRepository   products,
        IOrderRepository     orders,
        IPaymentGateway      payments,
        INotificationService notifications)
    {
        _products      = products;
        _orders        = orders;
        _payments      = payments;
        _notifications = notifications;
    }

    public async Task<OrderSummary> SubmitAsync(
        int orderId, string customerId, string productCode, int qty)
    {
        OrderValidator.Validate(orderId, customerId, productCode, qty);

        var product = await _products.GetByCodeAsync(productCode)
            ?? throw new InvalidOperationException($"Product '{productCode}' not found.");

        var discountRate = DiscountCalculator.GetRate(product.Category, qty);
        var total        = product.UnitPrice * qty * (1 - discountRate);

        var paid   = await _payments.ChargeAsync(customerId, total, orderId);
        var status = paid ? OrderStatus.Paid : OrderStatus.Pending;

        await _orders.SaveAsync(new Order(orderId, customerId, productCode, qty, total, status));

        if (paid)
            await _notifications.NotifyAsync(
                customerId, $"Order {orderId} confirmed. Total: {total:C}");

        return new OrderSummary(orderId, total, status);
    }
}

// ── Infrastructure (adapters) ─────────────────────────────────────────────────

public class SqlProductRepository : IProductRepository
{
    private readonly string _connectionString;

    public SqlProductRepository(string connectionString)
        => _connectionString = connectionString;

    public async Task<Product?> GetByCodeAsync(string code)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            "SELECT UnitPrice, Category FROM Products WHERE Code = @code", conn);
        cmd.Parameters.AddWithValue("@code", code);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new Product(code, (decimal)reader["UnitPrice"], (string)reader["Category"]);
    }
}

public class SqlOrderRepository : IOrderRepository
{
    private readonly string _connectionString;

    public SqlOrderRepository(string connectionString)
        => _connectionString = connectionString;

    public async Task SaveAsync(Order order)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            "INSERT INTO Orders (OrderId, CustomerId, ProductCode, Qty, Total, Status) " +
            "VALUES (@orderId, @customerId, @productCode, @qty, @total, @status)",
            conn);

        cmd.Parameters.AddWithValue("@orderId",     order.OrderId);
        cmd.Parameters.AddWithValue("@customerId",  order.CustomerId);
        cmd.Parameters.AddWithValue("@productCode", order.ProductCode);
        cmd.Parameters.AddWithValue("@qty",         order.Quantity);
        cmd.Parameters.AddWithValue("@total",       order.Total);
        cmd.Parameters.AddWithValue("@status",      order.Status.ToString().ToLowerInvariant());

        await cmd.ExecuteNonQueryAsync();
    }
}

public class HttpPaymentGateway : IPaymentGateway
{
    private readonly HttpClient _http;

    public HttpPaymentGateway(HttpClient http) => _http = http;

    public async Task<bool> ChargeAsync(string customerId, decimal amount, int orderId)
    {
        var body = JsonSerializer.Serialize(new { customerId, amount, reference = orderId });
        var response = await _http.PostAsync(
            "v1/charge",
            new StringContent(body, Encoding.UTF8, "application/json"));
        return response.IsSuccessStatusCode;
    }
}

public class HttpNotificationService : INotificationService
{
    private readonly HttpClient _http;

    public HttpNotificationService(HttpClient http) => _http = http;

    public async Task NotifyAsync(string customerId, string message)
    {
        var body = JsonSerializer.Serialize(new { to = customerId, text = message });
        await _http.PostAsync(
            "v1/push",
            new StringContent(body, Encoding.UTF8, "application/json"));
    }
}

// ── Composition root (Program.cs / DI registration) ──────────────────────────
//
// var connStr = builder.Configuration.GetConnectionString("Shop")!;
// builder.Services.AddScoped<IProductRepository>(_ => new SqlProductRepository(connStr));
// builder.Services.AddScoped<IOrderRepository>(_ => new SqlOrderRepository(connStr));
// builder.Services.AddHttpClient<IPaymentGateway, HttpPaymentGateway>(c =>
//     c.BaseAddress = new Uri(builder.Configuration["PaymentGateway:BaseUrl"]!));
// builder.Services.AddHttpClient<INotificationService, HttpNotificationService>(c =>
//     c.BaseAddress = new Uri(builder.Configuration["Notifications:BaseUrl"]!));
// builder.Services.AddScoped<OrderService>();
