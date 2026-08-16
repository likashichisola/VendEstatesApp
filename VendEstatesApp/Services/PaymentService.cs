using VendEstatesApp.Models;
using VendEstatesApp.Models.Enums;
using VendEstatesApp.Repositories;

namespace VendEstatesApp.Services;

public interface IPaymentService
{
    Task<IEnumerable<Payment>> GetAllAsync();

    Task<Payment?> GetByIdAsync(int id);

    Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status);

    Task<IEnumerable<Payment>> GetByTypeAsync(PaymentType type);

    Task<Payment> RecordAsync(Payment payment);

    Task<(bool Success, string? Error)> ApproveAsync(int paymentId, int processedByEmployeeId);

    Task<(bool Success, string? Error)> CompleteAsync(int paymentId);

    Task<(bool Success, string? Error)> RejectAsync(int paymentId);
}

/// <summary>
/// Records and tracks payments (booking, salary, expense, vehicle rental) through pending/approved/completed states.
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IVehicleBookingRepository _vehicleBookingRepository;
    private readonly INotificationService _notificationService;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IBookingRepository bookingRepository,
        IVehicleBookingRepository vehicleBookingRepository,
        INotificationService notificationService)
    {
        _paymentRepository = paymentRepository;
        _bookingRepository = bookingRepository;
        _vehicleBookingRepository = vehicleBookingRepository;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<Payment>> GetAllAsync() => await _paymentRepository.GetAllWithDetailsAsync();

    public async Task<Payment?> GetByIdAsync(int id) => await _paymentRepository.GetByIdWithDetailsAsync(id);

    public async Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status) => await _paymentRepository.GetByStatusAsync(status);

    public async Task<IEnumerable<Payment>> GetByTypeAsync(PaymentType type) => await _paymentRepository.GetByTypeAsync(type);

    public async Task<Payment> RecordAsync(Payment payment)
    {
        await _paymentRepository.AddAsync(payment);

        if (payment.Status == PaymentStatus.Completed || payment.Status == PaymentStatus.Approved)
        {
            await ApplyPaymentToSourceAsync(payment);
        }

        if (payment.EmployeeId.HasValue)
        {
            await _notificationService.NotifyAsync(payment.EmployeeId.Value, NotificationType.Payment,
                "Payment Recorded",
                $"A payment of {payment.Amount:N2} has been recorded for you.",
                "/Payment/Details/" + payment.Id);
        }

        return payment;
    }

    public async Task<(bool Success, string? Error)> ApproveAsync(int paymentId, int processedByEmployeeId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment is null)
        {
            return (false, "Payment not found.");
        }

        payment.Status = PaymentStatus.Approved;
        payment.ProcessedByEmployeeId = processedByEmployeeId;
        await _paymentRepository.UpdateAsync(payment);
        await ApplyPaymentToSourceAsync(payment);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> CompleteAsync(int paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment is null)
        {
            return (false, "Payment not found.");
        }

        payment.Status = PaymentStatus.Completed;
        await _paymentRepository.UpdateAsync(payment);
        await ApplyPaymentToSourceAsync(payment);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> RejectAsync(int paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment is null)
        {
            return (false, "Payment not found.");
        }

        payment.Status = PaymentStatus.Rejected;
        await _paymentRepository.UpdateAsync(payment);
        return (true, null);
    }

    private async Task ApplyPaymentToSourceAsync(Payment payment)
    {
        if (payment.BookingId.HasValue)
        {
            var booking = await _bookingRepository.GetByIdAsync(payment.BookingId.Value);
            if (booking is not null)
            {
                booking.AmountPaid += payment.Amount;
                await _bookingRepository.UpdateAsync(booking);
            }
        }

        if (payment.VehicleBookingId.HasValue)
        {
            var vehicleBooking = await _vehicleBookingRepository.GetByIdAsync(payment.VehicleBookingId.Value);
            if (vehicleBooking is not null)
            {
                vehicleBooking.AmountPaid += payment.Amount;
                await _vehicleBookingRepository.UpdateAsync(vehicleBooking);
            }
        }
    }
}
