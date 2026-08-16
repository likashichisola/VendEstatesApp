using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.Models;

/// <summary>
/// A payroll run for an employee for a given period, including statutory deductions and net salary.
/// </summary>
public class Payroll : BaseEntity
{
    public int EmployeeId { get; set; }

    public Employee? Employee { get; set; }

    public int PayrollMonth { get; set; }

    public int PayrollYear { get; set; }

    public PayrollStatus Status { get; set; } = PayrollStatus.Pending;

    public decimal BasicSalary { get; set; }

    public decimal Allowances { get; set; }

    public decimal GrossSalary { get; set; }

    public decimal PayeTax { get; set; }

    public decimal NapsaContribution { get; set; }

    public decimal NhimaContribution { get; set; }

    public decimal LoanDeduction { get; set; }

    public decimal AdvanceDeduction { get; set; }

    public decimal OtherDeductions { get; set; }

    public decimal TotalDeductions { get; set; }

    public decimal NetSalary { get; set; }

    public int? ApprovedByEmployeeId { get; set; }

    public Employee? ApprovedByEmployee { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public string? Notes { get; set; }

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
