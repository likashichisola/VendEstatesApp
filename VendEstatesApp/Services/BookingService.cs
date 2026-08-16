using VendEstatesApp.Models;
using VendEstatesApp.Models.Enums;
using VendEstatesApp.Repositories;

namespace VendEstatesApp.Services;

public interface IBookingService
{
    Task<IEnumerable<Booking>> GetAllAsync();

    Task<Booking?> GetByIdAsync(int id);

    Task<(bool Success, string? Error)> CreateAsync(Booking booking);

    Task<(bool Success, string? Error)> UpdateAsync(Booking booking);

    Task DeleteAsync(int id);

    Task<IEnumerable<Booking>> GetLongTermBookingsAsync();

    Task<IEnumerable<Booking>> GetByDateRangeAsync(DateTime start, DateTime end);

    Task<(bool Success, string? Error)> CheckInAsync(int bookingId);

    Task<(bool Success, string? Error)> CheckOutAsync(int bookingId);

    Task<(bool Success, string? Error)> CancelAsync(int bookingId);
}

/// <summary>
/// Booking CRUD with room availability validation, long-term stay detection (30+ days),
/// and room status synchronization on check-in/out/cancel.
/// </summary>
public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly INotificationService _notificationService;

    public BookingService(
        IBookingRepository bookingRepository,
        IRoomRepository roomRepository,
        IEmployeeRepository employeeRepository,
        INotificationService notificationService)
    {
        _bookingRepository = bookingRepository;
        _roomRepository = roomRepository;
        _employeeRepository = employeeRepository;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<Booking>> GetAllAsync() => await _bookingRepository.GetAllWithDetailsAsync();

    public async Task<Booking?> GetByIdAsync(int id) => await _bookingRepository.GetByIdWithDetailsAsync(id);

    public async Task<(bool Success, string? Error)> CreateAsync(Booking booking)
    {
        if (booking.CheckOutDate <= booking.CheckInDate)
        {
            return (false, "Check-out date must be after check-in date.");
        }

        var overlapping = await _bookingRepository.GetActiveBookingsForRoomAsync(booking.RoomId, booking.CheckInDate, booking.CheckOutDate);
        if (overlapping.Any())
        {
            return (false, "This room is already booked for the selected dates.");
        }

        await _bookingRepository.AddAsync(booking);

        var room = await _roomRepository.GetByIdAsync(booking.RoomId);
        if (room is not null && room.Status == RoomStatus.Available)
        {
            room.Status = RoomStatus.Reserved;
            await _roomRepository.UpdateAsync(room);
        }

        await NotifyManagersAsync(
            "New Booking Created",
            $"A new booking was created for {booking.GuestName} (Room {room?.RoomNumber}).",
            "/Booking/Details/" + booking.Id);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(Booking booking)
    {
        if (booking.CheckOutDate <= booking.CheckInDate)
        {
            return (false, "Check-out date must be after check-in date.");
        }

        var overlapping = await _bookingRepository.GetActiveBookingsForRoomAsync(booking.RoomId, booking.CheckInDate, booking.CheckOutDate, booking.Id);
        if (overlapping.Any())
        {
            return (false, "This room is already booked for the selected dates.");
        }

        await _bookingRepository.UpdateAsync(booking);
        return (true, null);
    }

    public async Task DeleteAsync(int id) => await _bookingRepository.DeleteAsync(id);

    public async Task<IEnumerable<Booking>> GetLongTermBookingsAsync() => await _bookingRepository.GetLongTermBookingsAsync();

    public async Task<IEnumerable<Booking>> GetByDateRangeAsync(DateTime start, DateTime end) => await _bookingRepository.GetByDateRangeAsync(start, end);

    public async Task<(bool Success, string? Error)> CheckInAsync(int bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking is null)
        {
            return (false, "Booking not found.");
        }

        booking.Status = BookingStatus.CheckedIn;
        await _bookingRepository.UpdateAsync(booking);

        var room = await _roomRepository.GetByIdAsync(booking.RoomId);
        if (room is not null)
        {
            room.Status = RoomStatus.Occupied;
            await _roomRepository.UpdateAsync(room);
        }

        await NotifyManagersAsync("Guest Checked In", $"{booking.GuestName} has checked in to Room {room?.RoomNumber}.", "/Booking/Details/" + booking.Id);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> CheckOutAsync(int bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking is null)
        {
            return (false, "Booking not found.");
        }

        booking.Status = BookingStatus.CheckedOut;
        await _bookingRepository.UpdateAsync(booking);

        var room = await _roomRepository.GetByIdAsync(booking.RoomId);
        if (room is not null)
        {
            room.Status = RoomStatus.Available;
            await _roomRepository.UpdateAsync(room);
        }

        await NotifyManagersAsync("Guest Checked Out", $"{booking.GuestName} has checked out of Room {room?.RoomNumber}.", "/Booking/Details/" + booking.Id);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> CancelAsync(int bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking is null)
        {
            return (false, "Booking not found.");
        }

        booking.Status = BookingStatus.Cancelled;
        await _bookingRepository.UpdateAsync(booking);

        var room = await _roomRepository.GetByIdAsync(booking.RoomId);
        if (room is not null && room.Status == RoomStatus.Reserved)
        {
            room.Status = RoomStatus.Available;
            await _roomRepository.UpdateAsync(room);
        }

        return (true, null);
    }

    private async Task NotifyManagersAsync(string title, string message, string linkUrl)
    {
        var employees = await _employeeRepository.GetAllWithBranchAsync();
        var managers = employees.Where(e => e.Role == EmployeeRole.Manager && e.IsActive);
        await _notificationService.NotifyRoleAsync(managers, NotificationType.Booking, title, message, linkUrl);
    }
}
