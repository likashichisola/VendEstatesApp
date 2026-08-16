using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.ViewModels.Employee;

public class EmployeeFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, Phone]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "Password")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Required]
    public EmployeeRole Role { get; set; }

    [Required, StringLength(80)]
    [Display(Name = "Job Title")]
    public string JobTitle { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Branch")]
    public int BranchId { get; set; }

    [Required, Range(0, double.MaxValue, ErrorMessage = "Basic salary must be a positive amount.")]
    [Display(Name = "Basic Salary")]
    [DataType(DataType.Currency)]
    public decimal BasicSalary { get; set; }

    [Required]
    [Display(Name = "Hire Date")]
    [DataType(DataType.Date)]
    public DateTime HireDate { get; set; } = DateTime.UtcNow;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "NAPSA Number")]
    public string? NapsaNumber { get; set; }

    [Display(Name = "NHIMA Number")]
    public string? NhimaNumber { get; set; }

    [Display(Name = "TPIN Number")]
    public string? TpinNumber { get; set; }

    public List<SelectListItem> BranchOptions { get; set; } = [];

    public List<SelectListItem> RoleOptions { get; set; } = [];

    public bool IsEdit => Id > 0;
}
