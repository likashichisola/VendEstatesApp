using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendEstatesApp.Constants;
using VendEstatesApp.Services;
using VendEstatesApp.ViewModels.Branch;
using BranchModel = VendEstatesApp.Models.Branch;

namespace VendEstatesApp.Controllers;

[Authorize(Roles = Roles.Director)]
public class BranchController : Controller
{
    private readonly IBranchService _branchService;

    public BranchController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    public async Task<IActionResult> Index()
    {
        var branches = await _branchService.GetAllAsync();
        return View(branches);
    }

    public async Task<IActionResult> Details(int id)
    {
        var branch = await _branchService.GetByIdAsync(id);
        if (branch is null)
        {
            return NotFound();
        }

        return View(branch);
    }

    public IActionResult Create() => View(new BranchFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BranchFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var branch = new BranchModel
        {
            Name = model.Name,
            Type = model.Type,
            Location = model.Location,
            ContactPhone = model.ContactPhone,
            ContactEmail = model.ContactEmail,
            IsActive = model.IsActive
        };

        await _branchService.CreateAsync(branch);
        TempData["SuccessMessage"] = "Branch created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var branch = await _branchService.GetByIdAsync(id);
        if (branch is null)
        {
            return NotFound();
        }

        var vm = new BranchFormViewModel
        {
            Id = branch.Id,
            Name = branch.Name,
            Type = branch.Type,
            Location = branch.Location,
            ContactPhone = branch.ContactPhone,
            ContactEmail = branch.ContactEmail,
            IsActive = branch.IsActive
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BranchFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var branch = new BranchModel
        {
            Id = model.Id,
            Name = model.Name,
            Type = model.Type,
            Location = model.Location,
            ContactPhone = model.ContactPhone,
            ContactEmail = model.ContactEmail,
            IsActive = model.IsActive
        };

        await _branchService.UpdateAsync(branch);
        TempData["SuccessMessage"] = "Branch updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _branchService.DeleteAsync(id);
        TempData["SuccessMessage"] = "Branch deleted.";
        return RedirectToAction(nameof(Index));
    }
}
