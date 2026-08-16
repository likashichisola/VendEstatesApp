using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.ViewModels.Room;

public class RoomFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(20)]
    [Display(Name = "Room Number")]
    public string RoomNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Room Type")]
    public RoomType Type { get; set; }

    [Display(Name = "Status")]
    public RoomStatus Status { get; set; } = RoomStatus.Available;

    [Required, Range(0, double.MaxValue, ErrorMessage = "Price must be a positive amount.")]
    [Display(Name = "Price Per Night")]
    [DataType(DataType.Currency)]
    public decimal PricePerNight { get; set; }

    [Required, Range(1, 20)]
    public int Capacity { get; set; } = 1;

    public string? Description { get; set; }

    [Required]
    [Display(Name = "Branch")]
    public int BranchId { get; set; }

    public List<SelectListItem> BranchOptions { get; set; } = [];

    public bool IsEdit => Id > 0;
}
