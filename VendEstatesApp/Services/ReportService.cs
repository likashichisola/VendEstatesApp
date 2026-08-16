using VendEstatesApp.Models;
using VendEstatesApp.Models.Enums;
using VendEstatesApp.Repositories;

namespace VendEstatesApp.Services;

public record IncomeExpenseReportRow(DateTime Period, decimal Income, decimal Expenses, decimal Net);

public record BranchExpenseSummary(string BranchName, decimal TotalAmount, int Count);

public interface IReportService
{
    Task<IEnumerable<Payment>> GetIncomePaymentsAsync(DateTime start, DateTime end);

    Task<IEnumerable<Expense>> GetExpensesAsync(DateTime start, DateTime end);

    Task<IEnumerable<IncomeExpenseReportRow>> GetMonthlyIncomeExpenseReportAsync(int year);

    Task<IEnumerable<BranchExpenseSummary>> GetExpensesByBranchAsync(DateTime start, DateTime end);

    Task<IEnumerable<Payroll>> GetPayrollReportAsync(int month, int year);
}

/// <summary>
/// Builds the expense/income and payroll reports consumed by Director/Manager/Accountant report pages.
/// </summary>
public class ReportService : IReportService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IExpenseRepository _expenseRepository;
    private readonly IPayrollRepository _payrollRepository;

    public ReportService(
        IPaymentRepository paymentRepository,
        IExpenseRepository expenseRepository,
        IPayrollRepository payrollRepository)
    {
        _paymentRepository = paymentRepository;
        _expenseRepository = expenseRepository;
        _payrollRepository = payrollRepository;
    }

    public async Task<IEnumerable<Payment>> GetIncomePaymentsAsync(DateTime start, DateTime end)
    {
        var payments = await _paymentRepository.GetAllWithDetailsAsync();
        return payments.Where(p => p.PaymentDate >= start && p.PaymentDate <= end
            && p.Type != PaymentType.SalaryPayment && p.Type != PaymentType.ExpensePayment
            && p.Status is PaymentStatus.Completed or PaymentStatus.Approved);
    }

    public async Task<IEnumerable<Expense>> GetExpensesAsync(DateTime start, DateTime end)
    {
        var expenses = await _expenseRepository.GetAllWithDetailsAsync();
        return expenses.Where(e => e.ExpenseDate >= start && e.ExpenseDate <= end
            && (e.Status == ExpenseStatus.Approved || e.Status == ExpenseStatus.Paid));
    }

    public async Task<IEnumerable<IncomeExpenseReportRow>> GetMonthlyIncomeExpenseReportAsync(int year)
    {
        var payments = await _paymentRepository.GetAllWithDetailsAsync();
        var expenses = await _expenseRepository.GetAllWithDetailsAsync();

        var rows = new List<IncomeExpenseReportRow>();
        for (var month = 1; month <= 12; month++)
        {
            var income = payments
                .Where(p => p.PaymentDate.Year == year && p.PaymentDate.Month == month
                    && p.Type != PaymentType.SalaryPayment && p.Type != PaymentType.ExpensePayment
                    && p.Status is PaymentStatus.Completed or PaymentStatus.Approved)
                .Sum(p => p.Amount);

            var expenseTotal = expenses
                .Where(e => e.ExpenseDate.Year == year && e.ExpenseDate.Month == month
                    && (e.Status == ExpenseStatus.Approved || e.Status == ExpenseStatus.Paid))
                .Sum(e => e.Amount);

            rows.Add(new IncomeExpenseReportRow(new DateTime(year, month, 1), income, expenseTotal, income - expenseTotal));
        }

        return rows;
    }

    public async Task<IEnumerable<BranchExpenseSummary>> GetExpensesByBranchAsync(DateTime start, DateTime end)
    {
        var expenses = await _expenseRepository.GetAllWithDetailsAsync();
        return expenses
            .Where(e => e.ExpenseDate >= start && e.ExpenseDate <= end
                && (e.Status == ExpenseStatus.Approved || e.Status == ExpenseStatus.Paid))
            .GroupBy(e => e.Branch?.Name ?? "Unknown")
            .Select(g => new BranchExpenseSummary(g.Key, g.Sum(e => e.Amount), g.Count()))
            .OrderByDescending(s => s.TotalAmount);
    }

    public async Task<IEnumerable<Payroll>> GetPayrollReportAsync(int month, int year) =>
        await _payrollRepository.GetByPeriodAsync(month, year);
}
