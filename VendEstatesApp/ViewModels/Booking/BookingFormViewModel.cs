using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VendEstatesApp.ViewModels.Booking;

public class BookingFormViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Room")]
    public int RoomId { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Guest Name")]
    public string GuestName { get; set; } = string.Empty;

    [Required, Phone]
    [Display(Name = "Guest Phone")]
    public string GuestPhone { get; set; } = string.Empty;

    [EmailAddress]
    [Display(Name = "Guest Email")]
    public string? GuestEmail { get; set; }

    [Display(Name = "Guest ID Number")]
    public string? GuestIdNumber { get; set; }

    [Required]
    [Display(Name = "Check-In Date")]
    [DataType(DataType.Date)]
    public DateTime CheckInDate { get; set; } = DateTime.Today;

    [Required]
    [Display(Name = "Check-Out Date")]
    [DataType(DataType.Date)]
    public DateTime CheckOutDate { get; set; } = DateTime.Today.AddDays(1);

    [Required, Range(1, 20)]
    [Display(Name = "Number of Guests")]
    public int NumberOfGuests { get; set; } = 1;

    [Required, Range(0, double.MaxValue, ErrorMessage = "Total amount must be a positive amount.")]
    [Display(Name = "Total Amount")]
    [DataType(DataType.Currency)]
    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }

    public List<SelectListItem> RoomOptions { get; set; } = [];

    public bool IsEdit => Id > 0;
}
