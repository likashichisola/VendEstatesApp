using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VendEstatesApp.Constants;
using VendEstatesApp.Services;
using VendEstatesApp.ViewModels.AgroInventory;
using AgroInventoryModel = VendEstatesApp.Models.AgroInventory;

namespace VendEstatesApp.Controllers;

[Authorize(Roles = Roles.All)]
public class AgroInventoryController : Controller
{
    private readonly IAgroInventoryService _agroInventoryService;
    private readonly IBranchService _branchService;

    public AgroInventoryController(IAgroInventoryService agroInventoryService, IBranchService branchService)
    {
        _agroInventoryService = agroInventoryService;
        _branchService = branchService;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _agroInventoryService.GetAllAsync();
        return View(items);
    }

    public async Task<IActionResult> LowStock()
    {
        var items = await _agroInventoryService.GetLowStockAsync();
        return View(items);
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _agroInventoryService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        return View(item);
    }

    [Authorize(Roles = Roles.DirectorOrManager)]
    public async Task<IActionResult> Create()
    {
        var vm = new AgroInventoryFormViewModel();
        await PopulateOptionsAsync(vm);
        return View(vm);
    }

    [Authorize(Roles = Roles.DirectorOrManager)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AgroInventoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model);
            return View(model);
        }

        var item = new AgroInventoryModel
        {
            ItemName = model.ItemName,
            Category = model.Category,
            Unit = model.Unit,
            QuantityInStock = model.QuantityInStock,
            UnitCost = model.UnitCost,
            ReorderLevel = model.ReorderLevel,
            Notes = model.Notes,
            BranchId = model.BranchId
        };

        await _agroInventoryService.CreateAsync(item);
        TempData["SuccessMessage"] = "Inventory item created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.DirectorOrManager)]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _agroInventoryService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        var vm = new AgroInventoryFormViewModel
        {
            Id = item.Id,
            ItemName = item.ItemName,
            Category = item.Category,
            Unit = item.Unit,
            QuantityInStock = item.QuantityInStock,
            UnitCost = item.UnitCost,
            ReorderLevel = item.ReorderLevel,
            Notes = item.Notes,
            BranchId = item.BranchId
        };

        await PopulateOptionsAsync(vm);
        return View(vm);
    }

    [Authorize(Roles = Roles.DirectorOrManager)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AgroInventoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model);
            return View(model);
        }

        var existing = await _agroInventoryService.GetByIdAsync(model.Id);
        if (existing is null)
        {
            return NotFound();
        }

        existing.ItemName = model.ItemName;
        existing.Category = model.Category;
        existing.Unit = model.Unit;
        existing.QuantityInStock = model.QuantityInStock;
        existing.UnitCost = model.UnitCost;
        existing.ReorderLevel = model.ReorderLevel;
        existing.Notes = model.Notes;
        existing.BranchId = model.BranchId;

        await _agroInventoryService.UpdateAsync(existing);
        TempData["SuccessMessage"] = "Inventory item updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Director)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _agroInventoryService.DeleteAsync(id);
        TempData["SuccessMessage"] = "Inventory item deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateOptionsAsync(AgroInventoryFormViewModel model)
    {
        var branches = await _branchService.GetActiveAsync();
        model.BranchOptions = branches.Select(b => new SelectListItem(b.Name, b.Id.ToString())).ToList();
    }
}
