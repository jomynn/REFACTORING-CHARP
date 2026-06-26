// =============================================================================
// MOCK EXAM #05 — ANSWER KEY
// Smells fixed:
//   1. Static mutable state   → IMemoryCache injected (thread-safe, no static)
//   2. SQL injection          → parameterized query with @id
//   3. Exception swallowing   → exceptions propagate; caller handles them
//   4. Hardcoded credentials  → connection string injected via constructor
//   5. No interface           → IProductCatalog + IProductRepository introduced
//   6. SRP violation          → CachingProductCatalog / SqlProductRepository / PriceMarkupPolicy separated
// =============================================================================

using System;
using System.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;

namespace CleanStore
{
    // ---------------------------------------------------------------------------
    // Domain record
    // ---------------------------------------------------------------------------

    public record Product(int Id, string Name, decimal BasePrice);

    // ---------------------------------------------------------------------------
    // Fix 5 — interfaces introduced so all dependencies are mockable
    // ---------------------------------------------------------------------------

    public interface IProductRepository
    {
        Product? GetById(int id);
    }

    public interface IProductCatalog
    {
        Product? GetProduct(int id);
    }

    // ---------------------------------------------------------------------------
    // Fix 2 & 4 — parameterized SQL; connection string injected, not hardcoded
    // ---------------------------------------------------------------------------

    public sealed class SqlProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        public SqlProductRepository(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            _connectionString = connectionString;
        }

        // Fix 3 — no catch block; exceptions propagate to the caller
        public Product? GetById(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            // Fix 2: named parameter @id — no string interpolation of user input
            const string sql = "SELECT Id, Name, Price FROM Products WHERE Id = @id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return new Product(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetDecimal(2)
            );
        }
    }

    // ---------------------------------------------------------------------------
    // Fix 6 (partial) — markup logic isolated in its own policy class
    // ---------------------------------------------------------------------------

    public static class PriceMarkupPolicy
    {
        private const decimal MarkupRate = 0.10m;

        public static decimal Apply(decimal basePrice) =>
            Math.Round(basePrice * (1m + MarkupRate), 2);
    }

    // ---------------------------------------------------------------------------
    // Fix 1, 5, 6 — caching + orchestration separated from SQL + markup
    //   - IMemoryCache is injected (thread-safe, no static dictionary)
    //   - GetProduct only coordinates; it does not know SQL or markup details
    // ---------------------------------------------------------------------------

    public sealed class CachingProductCatalog : IProductCatalog
    {
        private readonly IProductRepository _repository;
        private readonly IMemoryCache _cache;

        public CachingProductCatalog(IProductRepository repository, IMemoryCache cache)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _cache      = cache      ?? throw new ArgumentNullException(nameof(cache));
        }

        public Product? GetProduct(int id)
        {
            if (_cache.TryGetValue(id, out Product? cached))
                return cached;

            var product = _repository.GetById(id);

            if (product is null)
                return null;

            // Apply markup and cache the result
            var markedUpPrice   = PriceMarkupPolicy.Apply(product.BasePrice);
            var markedUpProduct = product with { BasePrice = markedUpPrice };

            _cache.Set(id, markedUpProduct);

            return markedUpProduct;
        }
    }
}
