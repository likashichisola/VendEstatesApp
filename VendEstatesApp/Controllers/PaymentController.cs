using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VendEstatesApp.Constants;
using VendEstatesApp.Models.Enums;
using VendEstatesApp.Services;
using VendEstatesApp.ViewModels.Payment;
using PaymentModel = VendEstatesApp.Models.Payment;

namespace VendEstatesApp.Controllers;

[Authorize(Roles = Roles.All)]
public class PaymentController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly IBookingService _bookingService;
    private readonly IVehicleBookingService _vehicleBookingService;
    private readonly IExpenseService _expenseService;
    private readonly IPayrollService _payrollService;

    public PaymentController(
        IPaymentService paymentService,
        IBookingService bookingService,
        IVehicleBookingService vehicleBookingService,
        IExpenseService expenseService,
        IPayrollService payrollService)
    {
        _paymentService = paymentService;
        _bookingService = bookingService;
        _vehicleBookingService = vehicleBookingService;
        _expenseService = expenseService;
        _payrollService = payrollService;
    }

    public async Task<IActionResult> Index()
    {
        var payments = await _paymentService.GetAllAsync();
        return View(payments);
    }

    public async Task<IActionResult> Details(int id)
    {
        var payment = await _paymentService.GetByIdAsync(id);
        if (payment is null)
        {
            return NotFound();
        }

        return View(payment);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new PaymentFormViewModel();
        await PopulateOptionsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PaymentFormViewModel model)
    {
        if (model.Type == PaymentType.BookingPayment && model.BookingId is null)
        {
            ModelState.AddModelError(nameof(model.BookingId), "Please select a booking.");
        }
        else if (model.Type == PaymentType.VehicleRentalPayment && model.VehicleBookingId is null)
        {
            ModelState.AddModelError(nameof(model.VehicleBookingId), "Please select a vehicle booking.");
        }
        else if (model.Type == PaymentType.ExpensePayment && model.ExpenseId is null)
        {
            ModelState.AddModelError(nameof(model.ExpenseId), "Please select an expense.");
        }
        else if (model.Type == PaymentType.SalaryPayment && model.PayrollId is null)
        {
            ModelState.AddModelError(nameof(model.PayrollId), "Please select a payroll record.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model);
            return View(model);
        }

        var payment = new PaymentModel
        {
            Type = model.Type,
            Method = model.Method,
            Status = PaymentStatus.Completed,
            Amount = model.Amount,
            PaymentDate = model.PaymentDate,
            ReferenceNumber = model.ReferenceNumber,
            Notes = model.Notes,
        };

        switch (model.Type)
        {
            case PaymentType.BookingPayment:
                payment.BookingId = model.BookingId;
                break;
            case PaymentType.VehicleRentalPayment:
                payment.VehicleBookingId = model.VehicleBookingId;
                break;
            case PaymentType.ExpensePayment:
                payment.ExpenseId = model.ExpenseId;
                break;
            case PaymentType.SalaryPayment:
                payment.PayrollId = model.PayrollId;
                var payroll = await _payrollService.GetByIdAsync(model.PayrollId!.Value);
                payment.EmployeeId = payroll?.EmployeeId;
                break;
        }

        await _paymentService.RecordAsync(payment);
        TempData["SuccessMessage"] = "Payment recorded successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateOptionsAsync(PaymentFormViewModel model)
    {
        var bookings = await _bookingService.GetAllAsync();
        model.BookingOptions = bookings
            .Where(b => b.Balance > 0 && b.Status != Models.Enums.BookingStatus.Cancelled)
            .Select(b => new SelectListItem($"{b.GuestName} - Room {b.Room?.RoomNumber} (Balance: K{b.Balance:N2})", b.Id.ToString()))
            .ToList();

        var vehicleBookings = await _vehicleBookingService.GetAllAsync();
        model.VehicleBookingOptions = vehicleBookings
            .Where(v => v.Balance > 0 && v.Status != Models.Enums.VehicleBookingStatus.Cancelled)
            .Select(v => new SelectListItem($"{v.CustomerName} - {v.Vehicle?.RegistrationNumber} (Balance: K{v.Balance:N2})", v.Id.ToString()))
            .ToList();

        var expenses = await _expenseService.GetByStatusAsync(Models.Enums.ExpenseStatus.Approved);
        model.ExpenseOptions = expenses
            .Select(e => new SelectListItem($"{e.Title} - {e.Branch?.Name} (K{e.Amount:N2})", e.Id.ToString()))
            .ToList();

        var payrolls = await _payrollService.GetAllAsync();
        model.PayrollOptions = payrolls
            .Where(p => p.Status == Models.Enums.PayrollStatus.Approved)
            .Select(p => new SelectListItem($"{p.Employee?.FullName} - {p.PayrollMonth}/{p.PayrollYear} (K{p.NetSalary:N2})", p.Id.ToString()))
            .ToList();
    }
}
