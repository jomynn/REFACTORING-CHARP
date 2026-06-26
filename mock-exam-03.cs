// =============================================================================
// MOCK REFACTORING EXAM #03 — C# .NET
// Time limit: 60 minutes
// Task: Identify all code smells, then refactor the code below.
// Rules: preserve all existing behaviour, use .NET 8 idioms, no new NuGet packages.
// =============================================================================
//
// SMELLS TO FIND (fill in before you start coding):
//   1. SQL injection   -> look up product + persist order
//   2. Tight coupling  -> IProductsRepository + IChargePaymentEndPoint + INotifyCustomerEndPoint
//   3. SRP violation   -> validate, discount
//   4. Magic literals. -> category + configuration
//   5. Duplicated DB block -> single LookupProductAsync(), SaveOrderAsync()
//   6. Untestable design    → all I/O behind interfaces
//
// YOUR REFACTORED CODE GOES BELOW THE DASHED LINE AT THE BOTTOM.
// =============================================================================

using System.Data.SqlClient;
using System.Text;
using System.Text.Json;

namespace MessyShop;

public class OrderProcessor
{
    private const string Conn =
        "Server=db.internal;Database=Shop;User Id=sa;Password=Sh0p@dmin!;";

    public OrderSummary? Submit(int orderId, string customerId, string productCode, int qty)
    {
        // --- validate ---
        if (orderId <= 0) return null;
        if (string.IsNullOrEmpty(customerId)) return null;
        if (string.IsNullOrEmpty(productCode)) return null;
        if (qty <= 0) return null;

        // --- look up product ---
        decimal unitPrice = 0;
        string category = "";
        using (var conn = new SqlConnection(Conn))
        {
            conn.Open();
            var cmd = new SqlCommand(
                $"SELECT UnitPrice, Category FROM Products WHERE Code = '{productCode}'", conn);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                unitPrice = (decimal)reader["UnitPrice"];
                category  = (string)reader["Category"];
            }
        }
        if (unitPrice == 0)
            throw new Exception("product not found");

        // --- calculate discount ---
        decimal discountRate = 0;
        if (category == "electronics")
        {
            if (qty >= 10)      discountRate = 0.15m;
            else if (qty >= 5)  discountRate = 0.08m;
            else                discountRate = 0m;
        }
        else if (category == "clothing")
        {
            if (qty >= 10)      discountRate = 0.15m;
            else if (qty >= 5)  discountRate = 0.08m;
            else                discountRate = 0m;
        }
        else if (category == "books")
        {
            discountRate = 0.05m;
        }

        decimal subtotal = unitPrice * qty;
        decimal total    = subtotal - (subtotal * discountRate);

        // --- charge payment ---
        bool paid = false;
        try
        {
            var http = new HttpClient();
            var body = JsonSerializer.Serialize(
                new { customerId, amount = total, reference = orderId });
            var response = http.PostAsync(
                "https://pay.internal/v1/charge",
                new StringContent(body, Encoding.UTF8, "application/json")).Result;
            paid = response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine("payment error: " + ex.Message);
        }

        // --- persist order ---
        using (var conn = new SqlConnection(Conn))
        {
            conn.Open();
            var cmd = new SqlCommand(
                $"INSERT INTO Orders (OrderId, CustomerId, ProductCode, Qty, Total, Status) " +
                $"VALUES ({orderId}, '{customerId}', '{productCode}', {qty}, {total}, '{(paid ? "paid" : "pending")}')",
                conn);
            cmd.ExecuteNonQuery();
        }

        // --- notify customer ---
        if (paid)
        {
            var http = new HttpClient();
            var msg  = JsonSerializer.Serialize(
                new { to = customerId, text = $"Order {orderId} confirmed. Total: {total:C}" });
            http.PostAsync(
                "https://notify.internal/v1/push",
                new StringContent(msg, Encoding.UTF8, "application/json")).Wait();
        }

        Console.WriteLine($"order {orderId} processed — paid={paid}");
        return new OrderSummary(orderId, total, paid ? "paid" : "pending");
    }
}

public record OrderSummary(int OrderId, decimal Total, string Status);

// =============================================================================
// YOUR REFACTORED CODE BELOW THIS LINE
// -----------------------------------------------------------------------------

//using System.Data.SqlClient;
//using System.Text;
//using System.Text.Json;

namespace MessyShop;

public enum EnumCategory{Electronics, Clothing, Books }
public record Product(decimal UnitPrice, string Category, string Code);
public record Order(decimal OrderId, decimal CustomerId, string ProductCode, int Qty, decimal Total, string Status);
public class OrderService
{
    IProductRepository _product;
    IOrderRepository _order;

    public OrderService(IProductRepository product,
    IOrderRepository order)
    {
        _product = product;
        _order = order;

    }
    public async Task<OrderSummary> SubmitAsync(int orderId, string customerId, string productCode, int qty)
    {
        // validate
        OrderValidator.Validate(orderId, customerId, productCode, qty);

        // --- look up product ---
        var products = await _product.GetByProductCodesync(productCode)
         ?? throw new InvalidOperationException($"Product '{productCode}' not found.");


        decimal total = 0;
        bool paid = false;

        return new OrderSummary(orderId, total,  paid ? "paid" : "pending");
    }
}

//------------- Validate
public static class OrderValidator
{
    public static void Validate(int orderId, string customerId, string productCode, int qty)
    {
        if (orderId <= 0)
            throw new ArgumentException("OrderId must be positive.", nameof(orderId));
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Customer Id is required.", nameof(customerId));
        if (string.IsNullOrWhiteSpace(productCode))
            throw new ArgumentException("Product code is required.", nameof(productCode));
        if (qty <= 0)
            throw new ArgumentException("Qty must be positive.", nameof(qty));
    }
}

// Calculat

public static class InvoiceCalculation
{
    decimal discountRateA = 0.15m;
    decimal discountRateB = 0.08m;
    decimal discountRateC =0m;

    public static decimal Discount(int qty)
    {
        
        
    }
}

// Sql Database
// 1. define abstraction
public interface IProductRepository
{
    Task<Product?> GetByProductCodesync(string productCode);
}

public interface IOrderRepository
{
    Task<Order> SaveOrderAsync();
}

// 2. concrete implementation — infra layer only
public class SqlProductRepository : IProductRepository
{
    private readonly string _connString;

    public SqlUserRepository(IConfiguration config)
        => _connString = config.GetConnectionString("DefaultConnection")!;

    public async Task<User?> GetByIdAsync(int id)
    {
        await using var conn = new SqlConnection(_connString);
        // parameterized — no SQL injection
        return await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Id = @Id", new { Id = id });
    }

    public async Task SaveAsync(User user)
    {
        await using var conn = new SqlConnection(_connString);
        await conn.ExecuteAsync(
            "INSERT INTO Users (Name, Email) VALUES (@Name, @Email)",
            new { user.Name, user.Email });
    }
}

// Endpoint

// Interface
public interface IChargePaymentEndPoint
{
    Task<bool> PostAsync(string customerId, decimal amount, int orderId );
}

public interface INotificationService
{
    Task PostAsync(string customerId, string message);
}

public class HttpChargePaymentEndPoint : IChargePaymentEndPoint
{
    private readonly HttpClient _http;

    public HttpChargePaymentEndPoint(HttpClient http) => _http = http;
    public async Task<bool> PostAsync (string customerId, decimal amount, int orderId )
    {
        var body = JsonSerializer.Serialize(
            new { customerId, amount, reference = orderId });
        var response = await _http.PostAsync("v1/charge",
            new StringContent(body, Encoding.UTF8,
                "application/json"));
        return response.IsSuccessStatusCode;
    }
}


public class HttpNotificationService : INotificationService
{
    private readonly HttpClient _http;

    public HttpNotificationService(HttpClient http) => _http = http;

    public async Task PostAsync(string customerId, string message)
    {
        var body = JsonSerializer.Serialize(
            new { to = customerId, text = message });
        var response = await _http.PostAsync("v1/push",
            new StringContent(body, Encoding.UTF8,
                "application/json"));
        return response.IsSuccessStatusCode;
    }
}

// Program.cs