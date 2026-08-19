using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VendEstatesApp.Constants;
using VendEstatesApp.Services;
using VendEstatesApp.ViewModels.Employee;
using EmployeeModel = VendEstatesApp.Models.Employee;

namespace VendEstatesApp.Controllers;

[Authorize(Roles = Roles.All)]
public class EmployeeController : Controller
{
    private readonly IEmployeeService _employeeService;
    private readonly IBranchService _branchService;

    public EmployeeController(IEmployeeService employeeService, IBranchService branchService)
    {
        _employeeService = employeeService;
        _branchService = branchService;
    }

    public async Task<IActionResult> Index()
    {
        var employees = await _employeeService.GetAllAsync();
        return View(employees);
    }

    public async Task<IActionResult> Details(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        return View(employee);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new EmployeeFormViewModel();
        await PopulateOptionsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "Password is required for new employees.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model);
            return View(model);
        }

        var employee = new EmployeeModel
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Username = model.Username,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            Role = model.Role,
            JobTitle = model.JobTitle,
            BranchId = model.BranchId,
            BasicSalary = model.BasicSalary,
            HireDate = model.HireDate,
            IsActive = model.IsActive,
            NapsaNumber = model.NapsaNumber,
            NhimaNumber = model.NhimaNumber,
            TpinNumber = model.TpinNumber,
            ZraNumber = model.ZraNumber
        };

        var (success, error) = await _employeeService.CreateAsync(employee, model.Password!);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            await PopulateOptionsAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Employee profile created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        var vm = new EmployeeFormViewModel
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Username = employee.Username,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            Role = employee.Role,
            JobTitle = employee.JobTitle,
            BranchId = employee.BranchId,
            BasicSalary = employee.BasicSalary,
            HireDate = employee.HireDate,
            IsActive = employee.IsActive,
            NapsaNumber = employee.NapsaNumber,
            NhimaNumber = employee.NhimaNumber,
            TpinNumber = employee.TpinNumber,
            ZraNumber = employee.ZraNumber
        };

        await PopulateOptionsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EmployeeFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model);
            return View(model);
        }

        var employee = new EmployeeModel
        {
            Id = model.Id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Username = model.Username,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            Role = model.Role,
            JobTitle = model.JobTitle,
            BranchId = model.BranchId,
            BasicSalary = model.BasicSalary,
            HireDate = model.HireDate,
            IsActive = model.IsActive,
            NapsaNumber = model.NapsaNumber,
            NhimaNumber = model.NhimaNumber,
            TpinNumber = model.TpinNumber,
            ZraNumber = model.ZraNumber
        };

        var (success, error) = await _employeeService.UpdateAsync(employee);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            await PopulateOptionsAsync(model);
            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            await _employeeService.ChangePasswordAsync(model.Id, model.Password);
        }

        TempData["SuccessMessage"] = "Employee profile updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.All)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _employeeService.DeactivateAsync(id);
        TempData["SuccessMessage"] = "Employee deactivated.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.All)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _employeeService.DeleteAsync(id);
        TempData["SuccessMessage"] = "Employee deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateOptionsAsync(EmployeeFormViewModel model)
    {
        var branches = await _branchService.GetActiveAsync();
        model.BranchOptions = branches.Select(b => new SelectListItem(b.Name, b.Id.ToString())).ToList();
        model.RoleOptions =
        [
            new SelectListItem("Director", "Director"),
            new SelectListItem("Manager", "Manager"),
            new SelectListItem("Accountant", "Accountant"),
            new SelectListItem("Other", "Other")
        ];
    }
}
