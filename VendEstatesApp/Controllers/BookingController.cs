using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VendEstatesApp.Constants;
using VendEstatesApp.Services;
using VendEstatesApp.ViewModels.Booking;
using BookingModel = VendEstatesApp.Models.Booking;

namespace VendEstatesApp.Controllers;

[Authorize(Roles = Roles.All)]
public class BookingController : Controller
{
    private readonly IBookingService _bookingService;
    private readonly IRoomService _roomService;

    public BookingController(IBookingService bookingService, IRoomService roomService)
    {
        _bookingService = bookingService;
        _roomService = roomService;
    }

    public async Task<IActionResult> Index()
    {
        var bookings = await _bookingService.GetAllAsync();
        return View(bookings);
    }

    public async Task<IActionResult> Details(int id)
    {
        var booking = await _bookingService.GetByIdAsync(id);
        if (booking is null)
        {
            return NotFound();
        }

        return View(booking);
    }

    public async Task<IActionResult> Calendar(DateTime? start)
    {
        var rangeStart = (start ?? DateTime.Today).AddDays(-(int)(start ?? DateTime.Today).DayOfWeek);
        var rangeEnd = rangeStart.AddDays(34);
        var bookings = await _bookingService.GetByDateRangeAsync(rangeStart, rangeEnd);
        ViewBag.RangeStart = rangeStart;
        ViewBag.RangeEnd = rangeEnd;
        return View(bookings);
    }

    public async Task<IActionResult> LongTermStays()
    {
        var bookings = await _bookingService.GetLongTermBookingsAsync();
        return View(bookings);
    }

    [Authorize(Roles = Roles.DirectorOrManager)]
    public async Task<IActionResult> Create()
    {
        var vm = new BookingFormViewModel();
        await PopulateOptionsAsync(vm);
        return View(vm);
    }

    [Authorize(Roles = Roles.DirectorOrManager)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model);
            return View(model);
        }

        var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var booking = new BookingModel
        {
            RoomId = model.RoomId,
            GuestName = model.GuestName,
            GuestPhone = model.GuestPhone,
            GuestEmail = model.GuestEmail,
            GuestIdNumber = model.GuestIdNumber,
            CheckInDate = model.CheckInDate,
            CheckOutDate = model.CheckOutDate,
            NumberOfGuests = model.NumberOfGuests,
            TotalAmount = model.TotalAmount,
            Notes = model.Notes,
            CreatedByEmployeeId = employeeId
        };

        var (success, error) = await _bookingService.CreateAsync(booking);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            await PopulateOptionsAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Booking created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.DirectorOrManager)]
    public async Task<IActionResult> Edit(int id)
    {
        var booking = await _bookingService.GetByIdAsync(id);
        if (booking is null)
        {
            return NotFound();
        }

        var vm = new BookingFormViewModel
        {
            Id = booking.Id,
            RoomId = booking.RoomId,
            GuestName = booking.GuestName,
            GuestPhone = booking.GuestPhone,
            GuestEmail = booking.GuestEmail,
            GuestIdNumber = booking.GuestIdNumber,
            CheckInDate = booking.CheckInDate,
            CheckOutDate = booking.CheckOutDate,
            NumberOfGuests = booking.NumberOfGuests,
            TotalAmount = booking.TotalAmount,
            Notes = booking.Notes
        };

        await PopulateOptionsAsync(vm);
        return View(vm);
    }

    [Authorize(Roles = Roles.DirectorOrManager)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BookingFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model);
            return View(model);
        }

        var existing = await _bookingService.GetByIdAsync(model.Id);
        if (existing is null)
        {
            return NotFound();
        }

        existing.RoomId = model.RoomId;
        existing.GuestName = model.GuestName;
        existing.GuestPhone = model.GuestPhone;
        existing.GuestEmail = model.GuestEmail;
        existing.GuestIdNumber = model.GuestIdNumber;
        existing.CheckInDate = model.CheckInDate;
        existing.CheckOutDate = model.CheckOutDate;
        existing.NumberOfGuests = model.NumberOfGuests;
        existing.TotalAmount = model.TotalAmount;
        existing.Notes = model.Notes;

        var (success, error) = await _bookingService.UpdateAsync(existing);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            await PopulateOptionsAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Booking updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.DirectorOrManager)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(int id)
    {
        var (success, error) = await _bookingService.CheckInAsync(id);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Guest checked in." : error;
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = Roles.DirectorOrManager)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut(int id)
    {
        var (success, error) = await _bookingService.CheckOutAsync(id);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Guest checked out." : error;
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = Roles.DirectorOrManager)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var (success, error) = await _bookingService.CancelAsync(id);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Booking cancelled." : error;
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task PopulateOptionsAsync(BookingFormViewModel model)
    {
        var rooms = await _roomService.GetAllAsync();
        model.RoomOptions = rooms.Select(r => new SelectListItem($"{r.RoomNumber} ({r.Type}) - {r.Branch?.Name}", r.Id.ToString())).ToList();
    }
}
