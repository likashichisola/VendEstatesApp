using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.Models;

/// <summary>
/// A vehicle owned by Rocks Car Rental available for rental bookings.
/// </summary>
public class Vehicle : BaseEntity
{
    public string RegistrationNumber { get; set; } = string.Empty;

    public string Make { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int Year { get; set; }

    public VehicleCategory Category { get; set; }

    public VehicleStatus Status { get; set; } = VehicleStatus.Available;

    public decimal DailyRate { get; set; }

    public string? Color { get; set; }

    public string? Notes { get; set; }

    public int BranchId { get; set; }

    public Branch? Branch { get; set; }

    public ICollection<VehicleBooking> Bookings { get; set; } = new List<VehicleBooking>();
}
