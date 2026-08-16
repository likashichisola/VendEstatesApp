using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.Models;

/// <summary>
/// An in-app notification shown in the notification dropdown for a specific employee/user.
/// </summary>
public class Notification : BaseEntity
{
    public int EmployeeId { get; set; }

    public Employee? Employee { get; set; }

    public NotificationType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? LinkUrl { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }
}
