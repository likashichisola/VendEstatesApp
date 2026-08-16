using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.ViewModels.Vehicle;

public class VehicleFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(20)]
    [Display(Name = "Registration Number")]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Make { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Model { get; set; } = string.Empty;

    [Required, Range(1980, 2100)]
    public int Year { get; set; } = DateTime.Today.Year;

    [Required]
    public VehicleCategory Category { get; set; }

    [Required]
    public VehicleStatus Status { get; set; } = VehicleStatus.Available;

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Daily rate must be greater than zero.")]
    [Display(Name = "Daily Rate")]
    public decimal DailyRate { get; set; }

    [StringLength(30)]
    public string? Color { get; set; }

    public string? Notes { get; set; }

    [Required]
    [Display(Name = "Branch")]
    public int BranchId { get; set; }

    public List<SelectListItem> BranchOptions { get; set; } = [];

    public bool IsEdit => Id != 0;
}
