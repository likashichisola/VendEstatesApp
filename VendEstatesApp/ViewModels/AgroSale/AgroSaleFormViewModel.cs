using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VendEstatesApp.ViewModels.AgroSale;

public class AgroSaleFormViewModel
{
    [Required]
    [Display(Name = "Inventory Item")]
    public int AgroInventoryId { get; set; }

    [StringLength(150)]
    [Display(Name = "Customer Name")]
    public string? CustomerName { get; set; }

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
    [Display(Name = "Quantity Sold")]
    public decimal QuantitySold { get; set; }

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than zero.")]
    [Display(Name = "Unit Price")]
    public decimal UnitPrice { get; set; }

    [Required]
    [Display(Name = "Sale Date")]
    [DataType(DataType.Date)]
    public DateTime SaleDate { get; set; } = DateTime.Today;

    public string? Notes { get; set; }

    public List<SelectListItem> InventoryOptions { get; set; } = [];
}
