// =============================================================================
// MOCK EXAM #11 — ANSWER KEY
// Smells fixed:
//   1. Magic numbers          → PayrollConstants static class with named decimal constants
//   2. DRY violation          → DeductionCalculator.Calculate(gross) called once from both branches
//   3. String discriminator   → EmployeeType enum (FullTime, PartTime)
//   4. No validation          → PayrollValidator.Validate() throws ArgumentException
//   5. SRP violation          → GrossPayCalculator / DeductionCalculator / PayslipFormatter / PayrollProcessor separated
//   6. Primitive return       → PayslipResult record (EmployeeName, GrossPay, Deductions, NetPay)
// =============================================================================

namespace CleanPayroll
{
    // Fix 3: enum instead of string discriminator
    public enum EmployeeType
    {
        FullTime,
        PartTime
    }

    // Fix 1: named constants replace all magic numbers
    public static class PayrollConstants
    {
        public const decimal IncomeTaxRate        = 0.22m;
        public const decimal SocialSecurityRate   = 0.065m;
        public const decimal StandardHours        = 40m;
        public const decimal OvertimeMultiplier   = 1.5m;
    }

    // Fix 6: structured result record instead of raw string
    public record Deductions(
        decimal IncomeTax,
        decimal SocialSecurity,
        decimal Total);

    public record PayslipResult(
        string EmployeeName,
        EmployeeType Type,
        decimal GrossPay,
        Deductions Deductions,
        decimal NetPay);

    // Fix 4: all input validation in one place
    public static class PayrollValidator
    {
        public static void Validate(decimal hourlyRate, decimal hoursWorked, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Employee name must not be empty.", nameof(name));

            if (hourlyRate <= 0m)
                throw new ArgumentException("Hourly rate must be greater than zero.", nameof(hourlyRate));

            if (hoursWorked < 0m)
                throw new ArgumentException("Hours worked must not be negative.", nameof(hoursWorked));
        }
    }

    // Fix 5: gross pay calculation extracted to its own class
    public static class GrossPayCalculator
    {
        public static decimal Calculate(EmployeeType type, decimal hourlyRate, decimal hoursWorked)
        {
            if (type == EmployeeType.FullTime && hoursWorked > PayrollConstants.StandardHours)
            {
                decimal regularPay  = PayrollConstants.StandardHours * hourlyRate;
                decimal overtimeHours = hoursWorked - PayrollConstants.StandardHours;
                decimal overtimePay = overtimeHours * hourlyRate * PayrollConstants.OvertimeMultiplier;
                return regularPay + overtimePay;
            }

            return hoursWorked * hourlyRate;
        }
    }

    // Fix 2 + Fix 5: deduction logic extracted; called once regardless of employee type
    public static class DeductionCalculator
    {
        public static Deductions Calculate(decimal grossPay)
        {
            decimal incomeTax       = grossPay * PayrollConstants.IncomeTaxRate;
            decimal socialSecurity  = grossPay * PayrollConstants.SocialSecurityRate;
            return new Deductions(incomeTax, socialSecurity, incomeTax + socialSecurity);
        }
    }

    // Fix 5 + Fix 6: formatting isolated from calculation
    public static class PayslipFormatter
    {
        public static string Format(PayslipResult result)
        {
            string typeLabel = result.Type == EmployeeType.FullTime ? "Full-Time" : "Part-Time";
            return
                $"PAYSLIP\n" +
                $"Employee  : {result.EmployeeName}\n" +
                $"Type      : {typeLabel}\n" +
                $"Gross Pay : {result.GrossPay:C}\n" +
                $"Income Tax: {result.Deductions.IncomeTax:C}\n" +
                $"Social Sec: {result.Deductions.SocialSecurity:C}\n" +
                $"Net Pay   : {result.NetPay:C}\n";
        }
    }

    // Fix 5: PayrollProcessor now only orchestrates; single responsibility
    public class PayrollProcessor
    {
        public PayslipResult Process(
            EmployeeType type,
            decimal hourlyRate,
            decimal hoursWorked,
            string employeeName)
        {
            PayrollValidator.Validate(hourlyRate, hoursWorked, employeeName);

            decimal gross      = GrossPayCalculator.Calculate(type, hourlyRate, hoursWorked);
            Deductions deductions = DeductionCalculator.Calculate(gross);
            decimal net        = gross - deductions.Total;

            return new PayslipResult(employeeName, type, gross, deductions, net);
        }
    }
}
