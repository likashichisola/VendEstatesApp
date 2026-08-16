using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VendEstatesApp.Constants;
using VendEstatesApp.Services;
using VendEstatesApp.ViewModels.VehicleBooking;
using VehicleBookingModel = VendEstatesApp.Models.VehicleBooking;

namespace VendEstatesApp.Controllers;

[Authorize(Roles = Roles.All)]
public class VehicleBookingController : Controller
{
    private readonly IVehicleBookingService _vehicleBookingService;
    private readonly IVehicleService _vehicleService;

    public VehicleBookingController(IVehicleBookingService vehicleBookingService, IVehicleService vehicleService)
    {
        _vehicleBookingService = vehicleBookingService;
        _vehicleService = vehicleService;
    }

    public async Task<IActionResult> Index()
    {
        var bookings = await _vehicleBookingService.GetAllAsync();
        return View(bookings);
    }

    public async Task<IActionResult> Details(int id)
    {
        var booking = await _vehicleBookingService.GetByIdAsync(id);
        if (booking is null)
        {
            return NotFound();
        }

        return View(booking);
    }

    [Authorize(Roles = Roles.DirectorOrManager)]
    public async Task<IActionResult> Create()
    {
        var vm = new VehicleBookingFormViewModel();
        await PopulateOptionsAsync(vm);
        return View(vm);
    }

    [Authorize(Roles = Roles.DirectorOrManager)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VehicleBookingFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model);
            return View(model);
        }

        var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var booking = new VehicleBookingModel
        {
            VehicleId = model.VehicleId,
            CustomerName = model.CustomerName,
            CustomerPhone = model.CustomerPhone,
            CustomerIdNumber = model.CustomerIdNumber,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            TotalAmount = model.TotalAmount,
            Notes = model.Notes,
            CreatedByEmployeeId = employeeId
        };

        var (success, error) = await _vehicleBookingService.CreateAsync(booking);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            await PopulateOptionsAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Vehicle booking created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.DirectorOrManager)]
    public async Task<IActionResult> Edit(int id)
    {
        var booking = await _vehicleBookingService.GetByIdAsync(id);
        if (booking is null)
        {
            return NotFound();
        }

        var vm = new VehicleBookingFormViewModel
        {
            Id = booking.Id,
            VehicleId = booking.VehicleId,
            CustomerName = booking.CustomerName,
            CustomerPhone = booking.CustomerPhone,
            CustomerIdNumber = booking.CustomerIdNumber,
            StartDate = booking.StartDate,
            EndDate = booking.EndDate,
            TotalAmount = booking.TotalAmount,
            Notes = booking.Notes
        };

        await PopulateOptionsAsync(vm);
        return View(vm);
    }

    [Authorize(Roles = Roles.DirectorOrManager)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(VehicleBookingFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model);
            return View(model);
        }

        var existing = await _vehicleBookingService.GetByIdAsync(model.Id);
        if (existing is null)
        {
            return NotFound();
        }

        existing.VehicleId = model.VehicleId;
        existing.CustomerName = model.CustomerName;
        existing.CustomerPhone = model.CustomerPhone;
        existing.CustomerIdNumber = model.CustomerIdNumber;
        existing.StartDate = model.StartDate;
        existing.EndDate = model.EndDate;
        existing.TotalAmount = model.TotalAmount;
        existing.Notes = model.Notes;

        var (success, error) = await _vehicleBookingService.UpdateAsync(existing);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            await PopulateOptionsAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Vehicle booking updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.DirectorOrManager)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        var (success, error) = await _vehicleBookingService.ActivateAsync(id);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Rental activated." : error;
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = Roles.DirectorOrManager)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        var (success, error) = await _vehicleBookingService.CompleteAsync(id);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Rental completed." : error;
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = Roles.DirectorOrManager)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var (success, error) = await _vehicleBookingService.CancelAsync(id);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Rental cancelled." : error;
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task PopulateOptionsAsync(VehicleBookingFormViewModel model)
    {
        var vehicles = await _vehicleService.GetAllAsync();
        model.VehicleOptions = vehicles
            .Select(v => new SelectListItem($"{v.RegistrationNumber} - {v.Make} {v.Model} ({v.Branch?.Name})", v.Id.ToString()))
            .ToList();
    }
}
