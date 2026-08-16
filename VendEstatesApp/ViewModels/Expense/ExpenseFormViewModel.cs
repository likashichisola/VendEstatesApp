using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.ViewModels.Expense;

public class ExpenseFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public ExpenseCategory Category { get; set; }

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required]
    [Display(Name = "Expense Date")]
    [DataType(DataType.Date)]
    public DateTime ExpenseDate { get; set; } = DateTime.Today;

    [Required]
    [Display(Name = "Branch")]
    public int BranchId { get; set; }

    public List<SelectListItem> BranchOptions { get; set; } = [];

    public bool IsEdit => Id != 0;
}
