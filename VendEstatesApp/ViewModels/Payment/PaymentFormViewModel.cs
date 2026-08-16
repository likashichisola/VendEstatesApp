using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.ViewModels.Payment;

public class PaymentFormViewModel
{
    [Required]
    public PaymentType Type { get; set; }

    [Required]
    public PaymentMethod Method { get; set; }

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required]
    [Display(Name = "Payment Date")]
    [DataType(DataType.Date)]
    public DateTime PaymentDate { get; set; } = DateTime.Today;

    [StringLength(100)]
    [Display(Name = "Reference Number")]
    public string? ReferenceNumber { get; set; }

    public string? Notes { get; set; }

    [Display(Name = "Booking")]
    public int? BookingId { get; set; }

    [Display(Name = "Vehicle Booking")]
    public int? VehicleBookingId { get; set; }

    [Display(Name = "Expense")]
    public int? ExpenseId { get; set; }

    [Display(Name = "Payroll")]
    public int? PayrollId { get; set; }

    public List<SelectListItem> BookingOptions { get; set; } = [];

    public List<SelectListItem> VehicleBookingOptions { get; set; } = [];

    public List<SelectListItem> ExpenseOptions { get; set; } = [];

    public List<SelectListItem> PayrollOptions { get; set; } = [];
}
