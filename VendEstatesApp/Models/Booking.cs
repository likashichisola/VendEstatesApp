using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.Models;

/// <summary>
/// A room booking/reservation at the Lodge. Bookings of 30+ days are treated as long-term stays.
/// </summary>
public class Booking : BaseEntity
{
    public int RoomId { get; set; }

    public Room? Room { get; set; }

    public string GuestName { get; set; } = string.Empty;

    public string GuestPhone { get; set; } = string.Empty;

    public string? GuestEmail { get; set; }

    public string? GuestIdNumber { get; set; }

    public DateTime CheckInDate { get; set; }

    public DateTime CheckOutDate { get; set; }

    public int NumberOfGuests { get; set; } = 1;

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public decimal TotalAmount { get; set; }

    public decimal AmountPaid { get; set; }

    public string? Notes { get; set; }

    public int CreatedByEmployeeId { get; set; }

    public Employee? CreatedByEmployee { get; set; }

    /// <summary>
    /// A stay is considered long-term when it spans 30 days or more.
    /// </summary>
    public int StayLengthInDays => (CheckOutDate.Date - CheckInDate.Date).Days;

    public bool IsLongTermStay => StayLengthInDays >= 30;

    public decimal Balance => TotalAmount - AmountPaid;

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
