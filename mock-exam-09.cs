// =============================================================================
// MOCK REFACTORING EXAM #09 — C# .NET
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
using System.Data.SqlClient;

namespace MessyWarehouse
{
    public class EmailService
    {
        public void SendAlert(string subject, string body)
        {
            // sends an email — implementation omitted
            Console.WriteLine($"[EmailService] Sending: {subject}");
        }
    }

    public class InventoryService
    {
        private readonly string _connectionString;

        public InventoryService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void DeductStock(string productId, int qty)
        {
            int currentStock = 0;

            // SMELL 2: SQL injection via string interpolation
            string selectSql = $"SELECT Stock FROM Products WHERE ProductId = '{productId}'";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var selectCmd = new SqlCommand(selectSql, connection))
                {
                    var result = selectCmd.ExecuteScalar();
                    if (result != null)
                        currentStock = (int)result;
                }

                // SMELL 1: Race condition — stock read separately, then deducted in a second statement.
                // A concurrent call can read the same currentStock before either UPDATE runs, overselling.
                if (currentStock < qty)
                {
                    // SMELL 3: Exception type misuse — generic Exception instead of a domain exception
                    throw new Exception("insufficient stock");
                }

                int newStock = currentStock - qty;

                // SMELL 2 (again): SQL injection in UPDATE
                string updateSql = $"UPDATE Products SET Stock = {newStock} WHERE ProductId = '{productId}'";

                using (var updateCmd = new SqlCommand(updateSql, connection))
                {
                    updateCmd.ExecuteNonQuery();
                }

                Console.WriteLine($"[InventoryService] Deducted {qty} units from product {productId}. New stock: {newStock}");

                // SMELL 4: Magic number — reorder threshold 5 hardcoded inline
                if (newStock < 5)
                {
                    // SMELL 5: Tight coupling — EmailService instantiated directly inside business logic
                    var emailService = new EmailService();

                    // SMELL 6: SRP violation — stock deduction method also handles alerting and logging
                    emailService.SendAlert(
                        subject: $"Low stock alert: {productId}",
                        body: $"Product {productId} has dropped to {newStock} units. Please reorder."
                    );
                }
            }
        }
    }
}

// =============================================================================
// YOUR REFACTORED CODE BELOW THIS LINE
// -----------------------------------------------------------------------------
