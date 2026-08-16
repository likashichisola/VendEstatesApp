using System.Security.Claims;
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
        if (User.IsInRole(Roles.Director) || User.IsInRole(Roles.Accountant))
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

    [Authorize(Roles = Roles.DirectorOrAccountant)]
    public async Task<IActionResult> Create()
    {
        var vm = new PayrollGenerateViewModel();
        await PopulateOptionsAsync(vm);
        return View(vm);
    }

    [Authorize(Roles = Roles.DirectorOrAccountant)]
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

    [Authorize(Roles = Roles.Director)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, error) = await _payrollService.ApproveAsync(id, employeeId);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Payroll approved." : error;
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = Roles.Director)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string reason)
    {
        var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, error) = await _payrollService.RejectAsync(id, employeeId, reason);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Payroll rejected." : error;
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = Roles.DirectorOrAccountant)]
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
