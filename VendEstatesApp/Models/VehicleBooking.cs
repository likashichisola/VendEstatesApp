using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.Models;

/// <summary>
/// A rental booking for a vehicle at Rocks Car Rental.
/// </summary>
public class VehicleBooking : BaseEntity
{
    public int VehicleId { get; set; }

    public Vehicle? Vehicle { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerPhone { get; set; } = string.Empty;

    public string? CustomerIdNumber { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public VehicleBookingStatus Status { get; set; } = VehicleBookingStatus.Pending;

    public decimal TotalAmount { get; set; }

    public decimal AmountPaid { get; set; }

    public string? Notes { get; set; }

    public int CreatedByEmployeeId { get; set; }

    public Employee? CreatedByEmployee { get; set; }

    public int RentalDays => Math.Max(1, (EndDate.Date - StartDate.Date).Days);

    public decimal Balance => TotalAmount - AmountPaid;

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
