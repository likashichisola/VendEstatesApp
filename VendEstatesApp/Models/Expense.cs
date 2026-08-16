using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.Models;

/// <summary>
/// An expense request raised by an accountant/manager, subject to Director approval.
/// </summary>
public class Expense : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ExpenseCategory Category { get; set; }

    public ExpenseStatus Status { get; set; } = ExpenseStatus.Pending;

    public decimal Amount { get; set; }

    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;

    public int BranchId { get; set; }

    public Branch? Branch { get; set; }

    public int RequestedByEmployeeId { get; set; }

    public Employee? RequestedByEmployee { get; set; }

    public int? ApprovedByEmployeeId { get; set; }

    public Employee? ApprovedByEmployee { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? RejectionReason { get; set; }

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
