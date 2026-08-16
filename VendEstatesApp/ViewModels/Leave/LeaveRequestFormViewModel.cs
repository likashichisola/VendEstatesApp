using System.ComponentModel.DataAnnotations;
using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.ViewModels.Leave;

public class LeaveRequestFormViewModel
{
    [Required]
    public LeaveType Type { get; set; }

    [Required]
    [Display(Name = "Start Date")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Required]
    [Display(Name = "End Date")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; } = DateTime.Today;

    [StringLength(500)]
    public string? Reason { get; set; }
}
