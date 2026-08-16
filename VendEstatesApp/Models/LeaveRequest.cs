using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.Models;

/// <summary>
/// A leave application submitted by an employee.
/// </summary>
public class LeaveRequest : BaseEntity
{
    public int EmployeeId { get; set; }

    public Employee? Employee { get; set; }

    public LeaveType Type { get; set; }

    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string? Reason { get; set; }

    public int? ApprovedByEmployeeId { get; set; }

    public Employee? ApprovedByEmployee { get; set; }

    public DateTime? DecisionAt { get; set; }

    public string? DecisionNotes { get; set; }

    public int NumberOfDays => (EndDate.Date - StartDate.Date).Days + 1;
}
