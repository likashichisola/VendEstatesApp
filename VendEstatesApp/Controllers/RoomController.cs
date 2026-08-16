using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VendEstatesApp.Constants;
using VendEstatesApp.Services;
using VendEstatesApp.ViewModels.Room;
using RoomModel = VendEstatesApp.Models.Room;

namespace VendEstatesApp.Controllers;

[Authorize(Roles = Roles.All)]
public class RoomController : Controller
{
    private readonly IRoomService _roomService;
    private readonly IBranchService _branchService;

    public RoomController(IRoomService roomService, IBranchService branchService)
    {
        _roomService = roomService;
        _branchService = branchService;
    }

    [Authorize(Roles = Roles.All)]
    public async Task<IActionResult> Index()
    {
        var rooms = await _roomService.GetAllAsync();
        return View(rooms);
    }

    [Authorize(Roles = Roles.All)]
    public async Task<IActionResult> Details(int id)
    {
        var room = await _roomService.GetByIdAsync(id);
        if (room is null)
        {
            return NotFound();
        }

        return View(room);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new RoomFormViewModel();
        await PopulateOptionsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoomFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model);
            return View(model);
        }

        var room = new RoomModel
        {
            RoomNumber = model.RoomNumber,
            Type = model.Type,
            Status = model.Status,
            PricePerNight = model.PricePerNight,
            Capacity = model.Capacity,
            Description = model.Description,
            BranchId = model.BranchId
        };

        var (success, error) = await _roomService.CreateAsync(room);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            await PopulateOptionsAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Room created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var room = await _roomService.GetByIdAsync(id);
        if (room is null)
        {
            return NotFound();
        }

        var vm = new RoomFormViewModel
        {
            Id = room.Id,
            RoomNumber = room.RoomNumber,
            Type = room.Type,
            Status = room.Status,
            PricePerNight = room.PricePerNight,
            Capacity = room.Capacity,
            Description = room.Description,
            BranchId = room.BranchId
        };

        await PopulateOptionsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(RoomFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model);
            return View(model);
        }

        var room = new RoomModel
        {
            Id = model.Id,
            RoomNumber = model.RoomNumber,
            Type = model.Type,
            Status = model.Status,
            PricePerNight = model.PricePerNight,
            Capacity = model.Capacity,
            Description = model.Description,
            BranchId = model.BranchId
        };

        var (success, error) = await _roomService.UpdateAsync(room);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            await PopulateOptionsAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Room updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _roomService.DeleteAsync(id);
        TempData["SuccessMessage"] = "Room deleted.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.All)]
    public async Task<IActionResult> Occupancy()
    {
        var summary = await _roomService.GetOccupancySummaryAsync();
        return View(summary);
    }

    private async Task PopulateOptionsAsync(RoomFormViewModel model)
    {
        var branches = await _branchService.GetActiveAsync();
        model.BranchOptions = branches.Select(b => new SelectListItem(b.Name, b.Id.ToString())).ToList();
    }
}
