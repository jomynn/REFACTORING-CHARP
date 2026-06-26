// =============================================================================
// MOCK REFACTORING EXAM #11 — C# .NET
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

namespace MessyPayroll
{
    public class PayrollProcessor
    {
        public string Process(
            string employeeType,
            decimal hourlyRate,
            decimal hoursWorked,
            string employeeName)
        {
            if (employeeType == "fulltime")
            {
                decimal gross;
                if (hoursWorked > 40m)
                {
                    decimal regularPay = 40m * hourlyRate;
                    decimal overtimeHours = hoursWorked - 40m;
                    decimal overtimePay = overtimeHours * hourlyRate * 1.5m;
                    gross = regularPay + overtimePay;
                }
                else
                {
                    gross = hoursWorked * hourlyRate;
                }

                decimal tax = gross * 0.22m;
                decimal ss = gross * 0.065m;
                decimal net = gross - tax - ss;

                string payslip =
                    $"PAYSLIP\n" +
                    $"Employee : {employeeName}\n" +
                    $"Type     : Full-Time\n" +
                    $"Gross Pay: {gross:C}\n" +
                    $"Income Tax: {tax:C}\n" +
                    $"Social Sec: {ss:C}\n" +
                    $"Net Pay  : {net:C}\n";

                return payslip;
            }
            else if (employeeType == "parttime")
            {
                decimal gross = hoursWorked * hourlyRate;

                decimal tax = gross * 0.22m;
                decimal ss = gross * 0.065m;
                decimal net = gross - tax - ss;

                string payslip =
                    $"PAYSLIP\n" +
                    $"Employee : {employeeName}\n" +
                    $"Type     : Part-Time\n" +
                    $"Gross Pay: {gross:C}\n" +
                    $"Income Tax: {tax:C}\n" +
                    $"Social Sec: {ss:C}\n" +
                    $"Net Pay  : {net:C}\n";

                return payslip;
            }
            else
            {
                return "Unknown employee type.";
            }
        }
    }
}

// =============================================================================
// YOUR REFACTORED CODE BELOW THIS LINE
// -----------------------------------------------------------------------------
