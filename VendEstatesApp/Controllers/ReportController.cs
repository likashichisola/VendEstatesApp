using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendEstatesApp.Constants;
using VendEstatesApp.Services;
using VendEstatesApp.ViewModels.Report;

namespace VendEstatesApp.Controllers;

[Authorize(Roles = Roles.All)]
public class ReportController : Controller
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> IncomeExpense(int? year)
    {
        var selectedYear = year ?? DateTime.UtcNow.Year;
        var rows = await _reportService.GetMonthlyIncomeExpenseReportAsync(selectedYear);

        var currentYear = DateTime.UtcNow.Year;
        var vm = new IncomeExpenseReportViewModel
        {
            Year = selectedYear,
            AvailableYears = Enumerable.Range(currentYear - 4, 5).Reverse().ToList(),
            Rows = rows.ToList()
        };

        return View(vm);
    }

    public async Task<IActionResult> BranchExpenses(DateTime? start, DateTime? end)
    {
        var startDate = start ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var endDate = end ?? startDate.AddMonths(1).AddDays(-1);

        var summaries = await _reportService.GetExpensesByBranchAsync(startDate, endDate.Date.AddDays(1).AddTicks(-1));

        var vm = new BranchExpenseReportViewModel
        {
            StartDate = startDate,
            EndDate = endDate,
            Summaries = summaries.ToList()
        };

        return View(vm);
    }

    [Authorize(Roles = Roles.All)]
    public async Task<IActionResult> Payroll(int? month, int? year)
    {
        var now = DateTime.UtcNow;
        var selectedMonth = month ?? now.Month;
        var selectedYear = year ?? now.Year;

        var payrolls = await _reportService.GetPayrollReportAsync(selectedMonth, selectedYear);

        var vm = new PayrollReportViewModel
        {
            Month = selectedMonth,
            Year = selectedYear,
            Payrolls = payrolls.ToList()
        };

        return View(vm);
    }
}
