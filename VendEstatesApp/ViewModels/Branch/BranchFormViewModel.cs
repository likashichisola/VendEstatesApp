using System.ComponentModel.DataAnnotations;
using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.ViewModels.Branch;

public class BranchFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public BranchType Type { get; set; }

    [StringLength(150)]
    public string? Location { get; set; }

    [Phone]
    [Display(Name = "Contact Phone")]
    public string? ContactPhone { get; set; }

    [EmailAddress]
    [Display(Name = "Contact Email")]
    public string? ContactEmail { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}
