using System.Security.Claims;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VendEstatesApp.Constants;
using VendEstatesApp.Services;
using VendEstatesApp.ViewModels.Payroll;

namespace VendEstatesApp.Controllers;

[Authorize(Roles = Roles.All)]
public class PayrollController : Controller
{
    private readonly IPayrollService _payrollService;
    private readonly IEmployeeService _employeeService;

    public PayrollController(IPayrollService payrollService, IEmployeeService employeeService)
    {
        _payrollService = payrollService;
        _employeeService = employeeService;
    }

    public async Task<IActionResult> Index()
    {
        if (User.IsInRole(Roles.Director) || User.IsInRole(Roles.Manager) || User.IsInRole(Roles.Accountant))
        {
            var all = await _payrollService.GetAllAsync();
            return View(all);
        }

        var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var mine = await _payrollService.GetByEmployeeAsync(employeeId);
        return View(mine);
    }

    public async Task<IActionResult> Details(int id)
    {
        var payroll = await _payrollService.GetByIdAsync(id);
        if (payroll is null)
        {
            return NotFound();
        }

        return View(payroll);
    }

    [Authorize(Roles = Roles.All)]
    public async Task<IActionResult> ExportToExcel()
    {
        var payrolls = (await _payrollService.GetAllAsync()).ToList();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Payroll");

        var headers = new[]
        {
            "Employee", "Job Title", "Branch", "Period", "Basic Salary", "Allowances", "Gross Salary",
            "PAYE Tax", "NAPSA", "NHIMA", "Loan Deduction", "Advance Deduction", "Other Deductions",
            "Total Deductions", "Net Salary", "Status"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
        }

        var row = 2;
        foreach (var payroll in payrolls)
        {
            worksheet.Cell(row, 1).Value = payroll.Employee?.FullName ?? string.Empty;
            worksheet.Cell(row, 2).Value = payroll.Employee?.JobTitle ?? string.Empty;
            worksheet.Cell(row, 3).Value = payroll.Employee?.Branch?.Name ?? string.Empty;
            worksheet.Cell(row, 4).Value = $"{payroll.PayrollMonth}/{payroll.PayrollYear}";
            worksheet.Cell(row, 5).Value = payroll.BasicSalary;
            worksheet.Cell(row, 6).Value = payroll.Allowances;
            worksheet.Cell(row, 7).Value = payroll.GrossSalary;
            worksheet.Cell(row, 8).Value = payroll.PayeTax;
            worksheet.Cell(row, 9).Value = payroll.NapsaContribution;
            worksheet.Cell(row, 10).Value = payroll.NhimaContribution;
            worksheet.Cell(row, 11).Value = payroll.LoanDeduction;
            worksheet.Cell(row, 12).Value = payroll.AdvanceDeduction;
            worksheet.Cell(row, 13).Value = payroll.OtherDeductions;
            worksheet.Cell(row, 14).Value = payroll.TotalDeductions;
            worksheet.Cell(row, 15).Value = payroll.NetSalary;
            worksheet.Cell(row, 16).Value = payroll.Status.ToString();
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"Payroll_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [Authorize(Roles = Roles.All)]
    public async Task<IActionResult> Create()
    {
        var vm = new PayrollGenerateViewModel();
        await PopulateOptionsAsync(vm);
        return View(vm);
    }

    [Authorize(Roles = Roles.All)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PayrollGenerateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model);
            return View(model);
        }

        var (success, error) = await _payrollService.GeneratePayrollAsync(
            model.EmployeeId,
            model.PayrollMonth,
            model.PayrollYear,
            model.Allowances,
            model.LoanDeduction,
            model.AdvanceDeduction,
            model.OtherDeductions,
            model.Notes);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            await PopulateOptionsAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Payroll generated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.All)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, error) = await _payrollService.ApproveAsync(id, employeeId);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Payroll approved." : error;
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = Roles.All)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string reason)
    {
        var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, error) = await _payrollService.RejectAsync(id, employeeId, reason);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Payroll rejected." : error;
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = Roles.All)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkPaid(int id)
    {
        var (success, error) = await _payrollService.MarkPaidAsync(id);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Payroll marked as paid." : error;
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task PopulateOptionsAsync(PayrollGenerateViewModel model)
    {
        var employees = await _employeeService.GetAllAsync();
        model.EmployeeOptions = employees
            .Where(e => e.IsActive)
            .Select(e => new SelectListItem($"{e.FullName} ({e.Role})", e.Id.ToString()))
            .ToList();
    }
}
