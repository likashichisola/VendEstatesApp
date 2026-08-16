using VendEstatesApp.Models.Enums;
using VendEstatesApp.Repositories;

namespace VendEstatesApp.Services;

public interface IDashboardService
{
    Task<DashboardSummary> GetSummaryAsync();
}

/// <summary>
/// Aggregates cross-module statistics for the role-based analytics dashboards.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IVehicleBookingRepository _vehicleBookingRepository;
    private readonly IExpenseRepository _expenseRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPayrollRepository _payrollRepository;
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IAgroInventoryRepository _agroInventoryRepository;

    public DashboardService(
        IEmployeeRepository employeeRepository,
        IBranchRepository branchRepository,
        IRoomRepository roomRepository,
        IBookingRepository bookingRepository,
        IVehicleRepository vehicleRepository,
        IVehicleBookingRepository vehicleBookingRepository,
        IExpenseRepository expenseRepository,
        IPaymentRepository paymentRepository,
        IPayrollRepository payrollRepository,
        ILeaveRequestRepository leaveRequestRepository,
        IAgroInventoryRepository agroInventoryRepository)
    {
        _employeeRepository = employeeRepository;
        _branchRepository = branchRepository;
        _roomRepository = roomRepository;
        _bookingRepository = bookingRepository;
        _vehicleRepository = vehicleRepository;
        _vehicleBookingRepository = vehicleBookingRepository;
        _expenseRepository = expenseRepository;
        _paymentRepository = paymentRepository;
        _payrollRepository = payrollRepository;
        _leaveRequestRepository = leaveRequestRepository;
        _agroInventoryRepository = agroInventoryRepository;
    }

    public async Task<DashboardSummary> GetSummaryAsync()
    {
        var employees = await _employeeRepository.GetAllAsync();
        var branches = await _branchRepository.GetAllAsync();
        var rooms = await _roomRepository.GetAllAsync();
        var bookings = await _bookingRepository.GetAllAsync();
        var vehicles = await _vehicleRepository.GetAllAsync();
        var vehicleBookings = await _vehicleBookingRepository.GetAllAsync();
        var expenses = await _expenseRepository.GetAllAsync();
        var payments = await _paymentRepository.GetAllAsync();
        var payrolls = await _payrollRepository.GetAllAsync();
        var leaveRequests = await _leaveRequestRepository.GetAllAsync();
        var agroItems = await _agroInventoryRepository.GetAllAsync();

        var now = DateTime.UtcNow;

        var incomeThisMonth = payments
            .Where(p => p.Status is PaymentStatus.Completed or PaymentStatus.Approved
                        && p.Type != PaymentType.SalaryPayment && p.Type != PaymentType.ExpensePayment
                        && p.PaymentDate.Year == now.Year && p.PaymentDate.Month == now.Month)
            .Sum(p => p.Amount);

        var expensesThisMonth = expenses
            .Where(e => (e.Status == ExpenseStatus.Approved || e.Status == ExpenseStatus.Paid)
                        && e.ExpenseDate.Year == now.Year && e.ExpenseDate.Month == now.Month)
            .Sum(e => e.Amount);

        return new DashboardSummary(
            TotalEmployees: employees.Count(e => e.IsActive),
            TotalBranches: branches.Count(),
            TotalRooms: rooms.Count(),
            AvailableRooms: rooms.Count(r => r.Status == RoomStatus.Available),
            OccupiedRooms: rooms.Count(r => r.Status == RoomStatus.Occupied),
            ReservedRooms: rooms.Count(r => r.Status == RoomStatus.Reserved),
            MaintenanceRooms: rooms.Count(r => r.Status == RoomStatus.Maintenance),
            TotalBookings: bookings.Count(),
            ActiveBookings: bookings.Count(b => b.Status == BookingStatus.CheckedIn || b.Status == BookingStatus.Pending),
            LongTermBookings: bookings.Count(b => b.IsLongTermStay),
            TotalVehicles: vehicles.Count(),
            AvailableVehicles: vehicles.Count(v => v.Status == VehicleStatus.Available),
            RentedVehicles: vehicles.Count(v => v.Status == VehicleStatus.Rented),
            ActiveVehicleBookings: vehicleBookings.Count(v => v.Status == VehicleBookingStatus.Active || v.Status == VehicleBookingStatus.Pending),
            TotalIncomeThisMonth: incomeThisMonth,
            TotalExpensesThisMonth: expensesThisMonth,
            NetIncomeThisMonth: incomeThisMonth - expensesThisMonth,
            PendingExpenseApprovals: expenses.Count(e => e.Status == ExpenseStatus.Pending),
            PendingPayrollApprovals: payrolls.Count(p => p.Status == PayrollStatus.Pending),
            PendingLeaveApprovals: leaveRequests.Count(l => l.Status == LeaveStatus.Pending),
            LowStockAgroItems: agroItems.Count(a => a.IsLowStock));
    }
}
