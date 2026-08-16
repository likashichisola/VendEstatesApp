using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.ViewModels.Contract;

public class ContractFormViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Employee")]
    public int EmployeeId { get; set; }

    [Required]
    [Display(Name = "Contract Type")]
    public ContractType Type { get; set; }

    [Display(Name = "Status")]
    public ContractStatus Status { get; set; } = ContractStatus.Active;

    [Required]
    [Display(Name = "Start Date")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    [Display(Name = "End Date")]
    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    [Required, Range(0, double.MaxValue, ErrorMessage = "Agreed salary must be a positive amount.")]
    [Display(Name = "Agreed Salary")]
    [DataType(DataType.Currency)]
    public decimal AgreedSalary { get; set; }

    [Display(Name = "Terms")]
    public string? Terms { get; set; }

    public List<SelectListItem> EmployeeOptions { get; set; } = [];

    public bool IsEdit => Id > 0;
}
