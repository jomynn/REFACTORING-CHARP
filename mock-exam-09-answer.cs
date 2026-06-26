// =============================================================================
// MOCK EXAM #09 — ANSWER KEY
// Smells fixed:
//   1. Race condition         → single atomic UPDATE SET stock = stock - @qty WHERE stock >= @qty; rows affected = 0 → exception
//   2. SQL injection          → parameterized @productId and @qty throughout
//   3. Exception type misuse  → InsufficientStockException : InvalidOperationException domain exception
//   4. Magic number           → ReorderThreshold constant in InventorySettings
//   5. Tight coupling         → IAlertService interface injected
//   6. SRP violation          → InventoryService / IStockRepository / IAlertService separated
// =============================================================================

using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace CleanWarehouse
{
    // -------------------------------------------------------------------------
    // Fix 3: Named domain exception instead of generic Exception
    // -------------------------------------------------------------------------
    public sealed class InsufficientStockException : InvalidOperationException
    {
        public string ProductId { get; }
        public int Requested { get; }
        public int Available { get; }

        public InsufficientStockException(string productId, int requested, int available)
            : base($"Cannot deduct {requested} units from product '{productId}': only {available} available.")
        {
            ProductId = productId;
            Requested = requested;
            Available = available;
        }
    }

    // -------------------------------------------------------------------------
    // Fix 6 + Fix 2 + Fix 1: IStockRepository abstracts data access;
    // atomic UPDATE eliminates the race condition; parameters eliminate injection
    // -------------------------------------------------------------------------
    public interface IStockRepository
    {
        /// <summary>
        /// Atomically deducts <paramref name="qty"/> units. Returns the new stock level.
        /// Throws <see cref="InsufficientStockException"/> when stock is insufficient.
        /// </summary>
        Task<int> DeductAsync(string productId, int qty);

        Task<int> GetStockAsync(string productId);
    }

    public sealed class SqlStockRepository : IStockRepository
    {
        private readonly string _connectionString;

        public SqlStockRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        // Fix 1: Single atomic UPDATE — race condition eliminated
        // Fix 2: @productId and @qty parameters — SQL injection eliminated
        public async Task<int> DeductAsync(string productId, int qty)
        {
            const string sql = """
                UPDATE Products
                SET    Stock = Stock - @qty
                OUTPUT INSERTED.Stock
                WHERE  ProductId = @productId
                  AND  Stock >= @qty
                """;

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@productId", productId);
            cmd.Parameters.AddWithValue("@qty", qty);

            var result = await cmd.ExecuteScalarAsync();

            if (result is null)
            {
                int current = await GetStockAsync(productId);
                throw new InsufficientStockException(productId, qty, current);
            }

            return (int)result;
        }

        public async Task<int> GetStockAsync(string productId)
        {
            const string sql = "SELECT Stock FROM Products WHERE ProductId = @productId";

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@productId", productId);

            var result = await cmd.ExecuteScalarAsync();
            return result is null ? 0 : (int)result;
        }
    }

    // -------------------------------------------------------------------------
    // Fix 5: IAlertService interface — tight coupling eliminated
    // -------------------------------------------------------------------------
    public interface IAlertService
    {
        Task SendLowStockAlertAsync(string productId, int currentStock);
    }

    // -------------------------------------------------------------------------
    // Fix 4: ReorderThreshold lives in a settings record — magic number eliminated
    // -------------------------------------------------------------------------
    public sealed record InventorySettings(int ReorderThreshold);

    // -------------------------------------------------------------------------
    // Fix 6: InventoryService has a single responsibility — orchestrate deduction
    // -------------------------------------------------------------------------
    public sealed class InventoryService
    {
        private readonly IStockRepository _repo;
        private readonly IAlertService _alerts;
        private readonly InventorySettings _settings;

        public InventoryService(
            IStockRepository repo,
            IAlertService alerts,
            InventorySettings settings)
        {
            _repo     = repo     ?? throw new ArgumentNullException(nameof(repo));
            _alerts   = alerts   ?? throw new ArgumentNullException(nameof(alerts));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task DeductAsync(string productId, int qty)
        {
            // Throws InsufficientStockException if stock is too low (atomic check+deduct in repo)
            int newStock = await _repo.DeductAsync(productId, qty);

            if (newStock < _settings.ReorderThreshold)
            {
                await _alerts.SendLowStockAlertAsync(productId, newStock);
            }
        }
    }
}
