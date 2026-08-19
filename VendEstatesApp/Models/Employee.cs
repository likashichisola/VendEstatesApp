using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.Models;

/// <summary>
/// A staff member and system user. Doubles as the authentication account (username/password hash).
/// </summary>
public class Employee : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public EmployeeRole Role { get; set; }

    public string JobTitle { get; set; } = string.Empty;

    public int BranchId { get; set; }

    public Branch? Branch { get; set; }

    public decimal BasicSalary { get; set; }

    public DateTime HireDate { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public string? NapsaNumber { get; set; }

    public string? NhimaNumber { get; set; }

    public string? TpinNumber { get; set; }

    public string? ZraNumber { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();

    public ICollection<Payroll> PayrollRecords { get; set; } = new List<Payroll>();

    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public ICollection<Expense> RequestedExpenses { get; set; } = new List<Expense>();

    public ICollection<Booking> BookingsCreated { get; set; } = new List<Booking>();

    public ICollection<PushSubscription> PushSubscriptions { get; set; } = new List<PushSubscription>();
}
