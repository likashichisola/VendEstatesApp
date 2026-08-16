namespace VendEstatesApp.Models;

/// <summary>
/// A browser/device push subscription registered by an employee for Web Push notifications.
/// Mirrors the subscription object returned by the PushManager API (endpoint + encryption keys).
/// </summary>
public class PushSubscription : BaseEntity
{
    public int EmployeeId { get; set; }

    public Employee? Employee { get; set; }

    public string Endpoint { get; set; } = string.Empty;

    public string P256dhKey { get; set; } = string.Empty;

    public string AuthKey { get; set; } = string.Empty;

    public string? UserAgent { get; set; }

    public DateTime? LastUsedAt { get; set; }
}
