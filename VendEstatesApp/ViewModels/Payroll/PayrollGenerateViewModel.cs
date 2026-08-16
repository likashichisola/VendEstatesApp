using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VendEstatesApp.ViewModels.Payroll;

public class PayrollGenerateViewModel
{
    [Required]
    [Display(Name = "Employee")]
    public int EmployeeId { get; set; }

    [Required, Range(1, 12)]
    [Display(Name = "Month")]
    public int PayrollMonth { get; set; } = DateTime.Today.Month;

    [Required, Range(2000, 2100)]
    [Display(Name = "Year")]
    public int PayrollYear { get; set; } = DateTime.Today.Year;

    [Range(0, double.MaxValue)]
    public decimal Allowances { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Loan Deduction")]
    public decimal LoanDeduction { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Advance Deduction")]
    public decimal AdvanceDeduction { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Other Deductions")]
    public decimal OtherDeductions { get; set; }

    public string? Notes { get; set; }

    public List<SelectListItem> EmployeeOptions { get; set; } = [];
}
