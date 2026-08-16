using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VendEstatesApp.Constants;
using VendEstatesApp.Services;
using VendEstatesApp.ViewModels.AgroSale;
using AgroSaleModel = VendEstatesApp.Models.AgroSale;

namespace VendEstatesApp.Controllers;

[Authorize(Roles = Roles.All)]
public class AgroSaleController : Controller
{
    private readonly IAgroSaleService _agroSaleService;
    private readonly IAgroInventoryService _agroInventoryService;

    public AgroSaleController(IAgroSaleService agroSaleService, IAgroInventoryService agroInventoryService)
    {
        _agroSaleService = agroSaleService;
        _agroInventoryService = agroInventoryService;
    }

    public async Task<IActionResult> Index()
    {
        var sales = await _agroSaleService.GetAllAsync();
        return View(sales);
    }

    [Authorize(Roles = Roles.All)]
    public async Task<IActionResult> Create()
    {
        var vm = new AgroSaleFormViewModel();
        await PopulateOptionsAsync(vm);
        return View(vm);
    }

    [Authorize(Roles = Roles.All)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AgroSaleFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model);
            return View(model);
        }

        var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var sale = new AgroSaleModel
        {
            AgroInventoryId = model.AgroInventoryId,
            CustomerName = model.CustomerName,
            QuantitySold = model.QuantitySold,
            UnitPrice = model.UnitPrice,
            SaleDate = model.SaleDate,
            Notes = model.Notes,
            SoldByEmployeeId = employeeId
        };

        var (success, error) = await _agroSaleService.RecordSaleAsync(sale);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            await PopulateOptionsAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Sale recorded successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateOptionsAsync(AgroSaleFormViewModel model)
    {
        var items = await _agroInventoryService.GetAllAsync();
        model.InventoryOptions = items
            .Where(i => i.QuantityInStock > 0)
            .Select(i => new SelectListItem($"{i.ItemName} ({i.QuantityInStock} {i.Unit} available)", i.Id.ToString()))
            .ToList();
    }
}
