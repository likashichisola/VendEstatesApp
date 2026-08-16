using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.Models;

/// <summary>
/// An employment contract for an employee (Director-managed).
/// </summary>
public class Contract : BaseEntity
{
    public int EmployeeId { get; set; }

    public Employee? Employee { get; set; }

    public ContractType Type { get; set; }

    public ContractStatus Status { get; set; } = ContractStatus.Active;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public decimal AgreedSalary { get; set; }

    public string? Terms { get; set; }

    public string? DocumentPath { get; set; }
}
