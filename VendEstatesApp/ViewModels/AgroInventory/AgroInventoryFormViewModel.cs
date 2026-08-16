using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.ViewModels.AgroInventory;

public class AgroInventoryFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Item Name")]
    public string ItemName { get; set; } = string.Empty;

    [Required]
    public AgroItemCategory Category { get; set; }

    [Required, StringLength(20)]
    public string Unit { get; set; } = "kg";

    [Required, Range(0, double.MaxValue)]
    [Display(Name = "Quantity In Stock")]
    public decimal QuantityInStock { get; set; }

    [Required, Range(0, double.MaxValue)]
    [Display(Name = "Unit Cost")]
    public decimal UnitCost { get; set; }

    [Required, Range(0, double.MaxValue)]
    [Display(Name = "Reorder Level")]
    public decimal ReorderLevel { get; set; }

    public string? Notes { get; set; }

    [Required]
    [Display(Name = "Branch")]
    public int BranchId { get; set; }

    public List<SelectListItem> BranchOptions { get; set; } = [];

    public bool IsEdit => Id != 0;
}
