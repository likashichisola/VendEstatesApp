using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.Models;

/// <summary>
/// A rentable room at the Vend Estates 8119 Lodge.
/// </summary>
public class Room : BaseEntity
{
    public string RoomNumber { get; set; } = string.Empty;

    public RoomType Type { get; set; }

    public RoomStatus Status { get; set; } = RoomStatus.Available;

    public decimal PricePerNight { get; set; }

    public int Capacity { get; set; }

    public string? Description { get; set; }

    public int BranchId { get; set; }

    public Branch? Branch { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
