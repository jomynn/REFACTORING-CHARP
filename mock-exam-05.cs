// =============================================================================
// MOCK REFACTORING EXAM #05 — C# .NET
// Time limit: 60 minutes
// Task: Identify all code smells, then refactor the code below.
// Rules: preserve all existing behaviour, use .NET 8 idioms, no new NuGet packages.
// =============================================================================
//
// SMELLS TO FIND (fill in before you start coding):
//   1. _______________________________________________
//   2. _______________________________________________
//   3. _______________________________________________
//   4. _______________________________________________
//   5. _______________________________________________
//   6. _______________________________________________
//
// YOUR REFACTORED CODE GOES BELOW THE DASHED LINE AT THE BOTTOM.
// =============================================================================

using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace MessyStore
{
    public record Product(int Id, string Name, decimal Price);

    public class ProductCatalog
    {
        // SMELL 1: static mutable state — shared across all instances, not thread-safe
        private static readonly Dictionary<int, Product> _cache = new();

        // SMELL 4: hardcoded credentials baked directly into source
        private const string ConnectionString =
            "Server=db.local;Database=Store;User Id=sa;Password=St0r3@dm1n!;";

        // SMELL 5: no interface — callers depend on the concrete class; cannot mock in tests
        // SMELL 6: SRP violation — one method does cache check, SQL fetch, markup, logging, and cache write
        public Product? GetProduct(int id)
        {
            if (_cache.TryGetValue(id, out var cached))
            {
                Console.WriteLine($"[Cache] hit for id={id}");
                return cached;
            }

            try
            {
                using var conn = new SqlConnection(ConnectionString);
                conn.Open();

                // SMELL 2: SQL injection — id interpolated directly into query string
                var sql = $"SELECT Id, Name, Price FROM Products WHERE Id = {id}";
                using var cmd = new SqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();

                if (!reader.Read())
                    return null;

                var product = new Product(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetDecimal(2)
                );

                // still inside GetProduct: apply 10% markup
                var markedUp = product with { Price = Math.Round(product.Price * 1.10m, 2) };

                Console.WriteLine($"[DB] fetched product id={id}, price after markup={markedUp.Price}");

                // still inside GetProduct: write to cache
                _cache[id] = markedUp;

                return markedUp;
            }
            catch (Exception ex)
            {
                // SMELL 3: exception swallowing — error is printed but execution silently continues
                Console.WriteLine("error: " + ex.Message);
                return null;
            }
        }
    }
}

// =============================================================================
// YOUR REFACTORED CODE BELOW THIS LINE
// -----------------------------------------------------------------------------
