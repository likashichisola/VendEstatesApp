using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VendEstatesApp.Constants;
using VendEstatesApp.Services;
using VendEstatesApp.ViewModels.Vehicle;
using VehicleModel = VendEstatesApp.Models.Vehicle;

namespace VendEstatesApp.Controllers;

[Authorize(Roles = Roles.All)]
public class VehicleController : Controller
{
    private readonly IVehicleService _vehicleService;
    private readonly IBranchService _branchService;

    public VehicleController(IVehicleService vehicleService, IBranchService branchService)
    {
        _vehicleService = vehicleService;
        _branchService = branchService;
    }

    public async Task<IActionResult> Index()
    {
        var vehicles = await _vehicleService.GetAllAsync();
        return View(vehicles);
    }

    public async Task<IActionResult> Details(int id)
    {
        var vehicle = await _vehicleService.GetByIdAsync(id);
        if (vehicle is null)
        {
            return NotFound();
        }

        return View(vehicle);
    }

    [Authorize(Roles = Roles.DirectorOrManager)]
    public async Task<IActionResult> Create()
    {
        var vm = new VehicleFormViewModel();
        await PopulateOptionsAsync(vm);
        return View(vm);
    }

    [Authorize(Roles = Roles.DirectorOrManager)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VehicleFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model);
            return View(model);
        }

        var vehicle = new VehicleModel
        {
            RegistrationNumber = model.RegistrationNumber,
            Make = model.Make,
            Model = model.Model,
            Year = model.Year,
            Category = model.Category,
            Status = model.Status,
            DailyRate = model.DailyRate,
            Color = model.Color,
            Notes = model.Notes,
            BranchId = model.BranchId
        };

        var (success, error) = await _vehicleService.CreateAsync(vehicle);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            await PopulateOptionsAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Vehicle added successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.DirectorOrManager)]
    public async Task<IActionResult> Edit(int id)
    {
        var vehicle = await _vehicleService.GetByIdAsync(id);
        if (vehicle is null)
        {
            return NotFound();
        }

        var vm = new VehicleFormViewModel
        {
            Id = vehicle.Id,
            RegistrationNumber = vehicle.RegistrationNumber,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Category = vehicle.Category,
            Status = vehicle.Status,
            DailyRate = vehicle.DailyRate,
            Color = vehicle.Color,
            Notes = vehicle.Notes,
            BranchId = vehicle.BranchId
        };

        await PopulateOptionsAsync(vm);
        return View(vm);
    }

    [Authorize(Roles = Roles.DirectorOrManager)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(VehicleFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model);
            return View(model);
        }

        var existing = await _vehicleService.GetByIdAsync(model.Id);
        if (existing is null)
        {
            return NotFound();
        }

        existing.RegistrationNumber = model.RegistrationNumber;
        existing.Make = model.Make;
        existing.Model = model.Model;
        existing.Year = model.Year;
        existing.Category = model.Category;
        existing.Status = model.Status;
        existing.DailyRate = model.DailyRate;
        existing.Color = model.Color;
        existing.Notes = model.Notes;
        existing.BranchId = model.BranchId;

        var (success, error) = await _vehicleService.UpdateAsync(existing);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            await PopulateOptionsAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Vehicle updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Director)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _vehicleService.DeleteAsync(id);
        TempData["SuccessMessage"] = "Vehicle deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateOptionsAsync(VehicleFormViewModel model)
    {
        var branches = await _branchService.GetActiveAsync();
        model.BranchOptions = branches.Select(b => new SelectListItem(b.Name, b.Id.ToString())).ToList();
    }
}
