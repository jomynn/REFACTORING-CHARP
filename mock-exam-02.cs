// =============================================================================
// MOCK REFACTORING EXAM #02 — C# .NET
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

        if (type == "standard")
        {
            if (amt > 10000)
                final = amt - (amt * 0.05m);
            else
                final = amt;

            var conn = new SqlConnection("Server=localhost;Database=AppDb;User Id=sa;Password=Admin123!;");
            conn.Open();
            var cmd = new SqlCommand($"INSERT INTO Invoices (CustomerId, Amount, Type, Status) VALUES ({id}, {final}, 'standard', 'pending')", conn);
            cmd.ExecuteNonQuery();
            conn.Close();

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
