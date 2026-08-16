using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VendEstatesApp.ViewModels.VehicleBooking;

public class VehicleBookingFormViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Vehicle")]
    public int VehicleId { get; set; }

    [Required, StringLength(150)]
    [Display(Name = "Customer Name")]
    public string CustomerName { get; set; } = string.Empty;

    [Required, Phone, StringLength(20)]
    [Display(Name = "Customer Phone")]
    public string CustomerPhone { get; set; } = string.Empty;

    [StringLength(50)]
    [Display(Name = "ID Number")]
    public string? CustomerIdNumber { get; set; }

    [Required]
    [Display(Name = "Start Date")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Required]
    [Display(Name = "End Date")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than zero.")]
    [Display(Name = "Total Amount")]
    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }

    public List<SelectListItem> VehicleOptions { get; set; } = [];

    public bool IsEdit => Id != 0;
}
