namespace VendEstatesApp.Models;

/// <summary>
/// Common auditable fields shared by every persisted entity in the system.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
