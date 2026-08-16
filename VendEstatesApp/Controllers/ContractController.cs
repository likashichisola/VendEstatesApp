using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VendEstatesApp.Constants;
using VendEstatesApp.Services;
using VendEstatesApp.ViewModels.Contract;
using ContractModel = VendEstatesApp.Models.Contract;

namespace VendEstatesApp.Controllers;

[Authorize(Roles = Roles.All)]
public class ContractController : Controller
{
    private readonly IContractService _contractService;
    private readonly IEmployeeService _employeeService;

    public ContractController(IContractService contractService, IEmployeeService employeeService)
    {
        _contractService = contractService;
        _employeeService = employeeService;
    }

    public async Task<IActionResult> Index()
    {
        var contracts = await _contractService.GetAllAsync();
        return View(contracts);
    }

    public async Task<IActionResult> Details(int id)
    {
        var contract = await _contractService.GetByIdAsync(id);
        if (contract is null)
        {
            return NotFound();
        }

        return View(contract);
    }

    [Authorize(Roles = Roles.All)]
    public async Task<IActionResult> Create()
    {
        var vm = new ContractFormViewModel();
        await PopulateOptionsAsync(vm);
        return View(vm);
    }

    [Authorize(Roles = Roles.All)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContractFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model);
            return View(model);
        }

        var contract = new ContractModel
        {
            EmployeeId = model.EmployeeId,
            Type = model.Type,
            Status = model.Status,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            AgreedSalary = model.AgreedSalary,
            Terms = model.Terms
        };

        await _contractService.CreateAsync(contract);
        TempData["SuccessMessage"] = "Contract created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.All)]
    public async Task<IActionResult> Edit(int id)
    {
        var contract = await _contractService.GetByIdAsync(id);
        if (contract is null)
        {
            return NotFound();
        }

        var vm = new ContractFormViewModel
        {
            Id = contract.Id,
            EmployeeId = contract.EmployeeId,
            Type = contract.Type,
            Status = contract.Status,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            AgreedSalary = contract.AgreedSalary,
            Terms = contract.Terms
        };

        await PopulateOptionsAsync(vm);
        return View(vm);
    }

    [Authorize(Roles = Roles.All)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ContractFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model);
            return View(model);
        }

        var contract = new ContractModel
        {
            Id = model.Id,
            EmployeeId = model.EmployeeId,
            Type = model.Type,
            Status = model.Status,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            AgreedSalary = model.AgreedSalary,
            Terms = model.Terms
        };

        await _contractService.UpdateAsync(contract);
        TempData["SuccessMessage"] = "Contract updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.All)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _contractService.DeleteAsync(id);
        TempData["SuccessMessage"] = "Contract deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateOptionsAsync(ContractFormViewModel model)
    {
        var employees = await _employeeService.GetAllAsync();
        model.EmployeeOptions = employees.Select(e => new SelectListItem(e.FullName, e.Id.ToString())).ToList();
    }
}
