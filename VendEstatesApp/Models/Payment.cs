using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.Models;

/// <summary>
/// A payment record: booking payment, employee salary payment, expense payment, or vehicle rental payment.
/// </summary>
public class Payment : BaseEntity
{
    public PaymentType Type { get; set; }

    public PaymentMethod Method { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    public string? ReferenceNumber { get; set; }

    public string? Notes { get; set; }

    public int? BookingId { get; set; }

    public Booking? Booking { get; set; }

    public int? VehicleBookingId { get; set; }

    public VehicleBooking? VehicleBooking { get; set; }

    public int? EmployeeId { get; set; }

    public Employee? Employee { get; set; }

    public int? ExpenseId { get; set; }

    public Expense? Expense { get; set; }

    public int? PayrollId { get; set; }

    public Payroll? Payroll { get; set; }

    public int? ProcessedByEmployeeId { get; set; }

    public Employee? ProcessedByEmployee { get; set; }
}
