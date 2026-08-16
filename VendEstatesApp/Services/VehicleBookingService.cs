using VendEstatesApp.Models;
using VendEstatesApp.Models.Enums;
using VendEstatesApp.Repositories;

namespace VendEstatesApp.Services;

public interface IVehicleBookingService
{
    Task<IEnumerable<VehicleBooking>> GetAllAsync();

    Task<VehicleBooking?> GetByIdAsync(int id);

    Task<(bool Success, string? Error)> CreateAsync(VehicleBooking booking);

    Task<(bool Success, string? Error)> UpdateAsync(VehicleBooking booking);

    Task DeleteAsync(int id);

    Task<(bool Success, string? Error)> ActivateAsync(int bookingId);

    Task<(bool Success, string? Error)> CompleteAsync(int bookingId);

    Task<(bool Success, string? Error)> CancelAsync(int bookingId);
}

/// <summary>
/// Vehicle rental booking CRUD with availability validation and vehicle status synchronization.
/// </summary>
public class VehicleBookingService : IVehicleBookingService
{
    private readonly IVehicleBookingRepository _vehicleBookingRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public VehicleBookingService(IVehicleBookingRepository vehicleBookingRepository, IVehicleRepository vehicleRepository)
    {
        _vehicleBookingRepository = vehicleBookingRepository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task<IEnumerable<VehicleBooking>> GetAllAsync() => await _vehicleBookingRepository.GetAllWithDetailsAsync();

    public async Task<VehicleBooking?> GetByIdAsync(int id) => await _vehicleBookingRepository.GetByIdWithDetailsAsync(id);

    public async Task<(bool Success, string? Error)> CreateAsync(VehicleBooking booking)
    {
        if (booking.EndDate <= booking.StartDate)
        {
            return (false, "End date must be after the start date.");
        }

        var overlapping = await _vehicleBookingRepository.GetActiveBookingsForVehicleAsync(booking.VehicleId, booking.StartDate, booking.EndDate);
        if (overlapping.Any())
        {
            return (false, "This vehicle is already booked for the selected dates.");
        }

        await _vehicleBookingRepository.AddAsync(booking);

        var vehicle = await _vehicleRepository.GetByIdAsync(booking.VehicleId);
        if (vehicle is not null && vehicle.Status == VehicleStatus.Available)
        {
            vehicle.Status = VehicleStatus.Rented;
            await _vehicleRepository.UpdateAsync(vehicle);
        }

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(VehicleBooking booking)
    {
        if (booking.EndDate <= booking.StartDate)
        {
            return (false, "End date must be after the start date.");
        }

        var overlapping = await _vehicleBookingRepository.GetActiveBookingsForVehicleAsync(booking.VehicleId, booking.StartDate, booking.EndDate, booking.Id);
        if (overlapping.Any())
        {
            return (false, "This vehicle is already booked for the selected dates.");
        }

        await _vehicleBookingRepository.UpdateAsync(booking);
        return (true, null);
    }

    public async Task DeleteAsync(int id) => await _vehicleBookingRepository.DeleteAsync(id);

    public async Task<(bool Success, string? Error)> ActivateAsync(int bookingId)
    {
        var booking = await _vehicleBookingRepository.GetByIdAsync(bookingId);
        if (booking is null)
        {
            return (false, "Booking not found.");
        }

        booking.Status = VehicleBookingStatus.Active;
        await _vehicleBookingRepository.UpdateAsync(booking);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> CompleteAsync(int bookingId)
    {
        var booking = await _vehicleBookingRepository.GetByIdAsync(bookingId);
        if (booking is null)
        {
            return (false, "Booking not found.");
        }

        booking.Status = VehicleBookingStatus.Completed;
        await _vehicleBookingRepository.UpdateAsync(booking);

        var vehicle = await _vehicleRepository.GetByIdAsync(booking.VehicleId);
        if (vehicle is not null)
        {
            vehicle.Status = VehicleStatus.Available;
            await _vehicleRepository.UpdateAsync(vehicle);
        }

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> CancelAsync(int bookingId)
    {
        var booking = await _vehicleBookingRepository.GetByIdAsync(bookingId);
        if (booking is null)
        {
            return (false, "Booking not found.");
        }

        booking.Status = VehicleBookingStatus.Cancelled;
        await _vehicleBookingRepository.UpdateAsync(booking);

        var vehicle = await _vehicleRepository.GetByIdAsync(booking.VehicleId);
        if (vehicle is not null && vehicle.Status == VehicleStatus.Rented)
        {
            vehicle.Status = VehicleStatus.Available;
            await _vehicleRepository.UpdateAsync(vehicle);
        }

        return (true, null);
    }
}
