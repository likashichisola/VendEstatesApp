using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VendEstatesApp.Constants;
using VendEstatesApp.Services;
using VendEstatesApp.ViewModels.Expense;
using ExpenseModel = VendEstatesApp.Models.Expense;

namespace VendEstatesApp.Controllers;

[Authorize(Roles = Roles.All)]
public class ExpenseController : Controller
{
    private readonly IExpenseService _expenseService;
    private readonly IBranchService _branchService;

    public ExpenseController(IExpenseService expenseService, IBranchService branchService)
    {
        _expenseService = expenseService;
        _branchService = branchService;
    }

    public async Task<IActionResult> Index()
    {
        var expenses = await _expenseService.GetAllAsync();
        return View(expenses);
    }

    public async Task<IActionResult> Details(int id)
    {
        var expense = await _expenseService.GetByIdAsync(id);
        if (expense is null)
        {
            return NotFound();
        }

        return View(expense);
    }

    [Authorize(Roles = Roles.All)]
    public async Task<IActionResult> Create()
    {
        var vm = new ExpenseFormViewModel();
        await PopulateOptionsAsync(vm);
        return View(vm);
    }

    [Authorize(Roles = Roles.All)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExpenseFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model);
            return View(model);
        }

        var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var expense = new ExpenseModel
        {
            Title = model.Title,
            Description = model.Description,
            Category = model.Category,
            Amount = model.Amount,
            ExpenseDate = model.ExpenseDate,
            BranchId = model.BranchId,
            RequestedByEmployeeId = employeeId
        };

        await _expenseService.SubmitAsync(expense);
        TempData["SuccessMessage"] = "Expense request submitted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.All)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, error) = await _expenseService.ApproveAsync(id, employeeId);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Expense approved." : error;
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = Roles.All)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string reason)
    {
        var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, error) = await _expenseService.RejectAsync(id, employeeId, reason);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Expense rejected." : error;
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = Roles.All)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkPaid(int id)
    {
        var (success, error) = await _expenseService.MarkPaidAsync(id);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Expense marked as paid." : error;
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = Roles.All)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _expenseService.DeleteAsync(id);
        TempData["SuccessMessage"] = "Expense deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateOptionsAsync(ExpenseFormViewModel model)
    {
        var branches = await _branchService.GetActiveAsync();
        model.BranchOptions = branches.Select(b => new SelectListItem(b.Name, b.Id.ToString())).ToList();
    }
}
