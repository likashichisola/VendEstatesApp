namespace VendEstatesApp.Services;

public record PayrollCalculationResult(
    decimal GrossSalary,
    decimal PayeTax,
    decimal NapsaContribution,
    decimal NhimaContribution,
    decimal TotalDeductions,
    decimal NetSalary);

public interface IPayrollCalculationService
{
    PayrollCalculationResult Calculate(
        decimal basicSalary,
        decimal allowances,
        decimal loanDeduction,
        decimal advanceDeduction,
        decimal otherDeductions);
}

/// <summary>
/// Calculates Zambian statutory payroll deductions (PAYE/ZRA, NAPSA, NHIMA) and net salary.
/// NOTE: Tax bands and rates below are illustrative/configurable constants. They must be reviewed
/// and updated periodically to reflect the current ZRA/NAPSA/NHIMA legislation in force.
/// </summary>
public class PayrollCalculationService : IPayrollCalculationService
{
    // NAPSA: 5% employee contribution, capped at a monthly ceiling.
    private const decimal NapsaRate = 0.05m;
    private const decimal NapsaMonthlyCeiling = 1421.80m;

    // NHIMA: 1% of basic salary, no ceiling.
    private const decimal NhimaRate = 0.01m;

    // PAYE (ZRA) monthly progressive tax bands: (UpperLimit, Rate)
    // Band 1: up to 5,100 -> 0%
    // Band 2: 5,100.01 - 7,100 -> 20%
    // Band 3: 7,100.01 - 9,200 -> 30%
    // Band 4: above 9,200 -> 37%
    private static readonly (decimal UpperLimit, decimal Rate)[] PayeBands =
    [
        (5100m, 0.00m),
        (7100m, 0.20m),
        (9200m, 0.30m),
        (decimal.MaxValue, 0.37m)
    ];

    public PayrollCalculationResult Calculate(
        decimal basicSalary,
        decimal allowances,
        decimal loanDeduction,
        decimal advanceDeduction,
        decimal otherDeductions)
    {
        var grossSalary = basicSalary + allowances;

        var napsaContribution = Math.Min(basicSalary * NapsaRate, NapsaMonthlyCeiling);
        var nhimaContribution = basicSalary * NhimaRate;
        var payeTax = CalculatePaye(grossSalary);

        var totalDeductions = payeTax + napsaContribution + nhimaContribution
            + loanDeduction + advanceDeduction + otherDeductions;

        var netSalary = grossSalary - totalDeductions;

        return new PayrollCalculationResult(
            GrossSalary: Math.Round(grossSalary, 2),
            PayeTax: Math.Round(payeTax, 2),
            NapsaContribution: Math.Round(napsaContribution, 2),
            NhimaContribution: Math.Round(nhimaContribution, 2),
            TotalDeductions: Math.Round(totalDeductions, 2),
            NetSalary: Math.Round(netSalary, 2));
    }

    private static decimal CalculatePaye(decimal grossSalary)
    {
        decimal tax = 0m;
        decimal previousLimit = 0m;

        foreach (var (upperLimit, rate) in PayeBands)
        {
            if (grossSalary <= previousLimit)
            {
                break;
            }

            var taxableInBand = Math.Min(grossSalary, upperLimit) - previousLimit;
            if (taxableInBand > 0)
            {
                tax += taxableInBand * rate;
            }

            previousLimit = upperLimit;
        }

        return tax;
    }
}
