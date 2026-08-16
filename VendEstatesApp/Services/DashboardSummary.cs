namespace VendEstatesApp.Services;

public record DashboardSummary(
    int TotalEmployees,
    int TotalBranches,
    int TotalRooms,
    int AvailableRooms,
    int OccupiedRooms,
    int ReservedRooms,
    int MaintenanceRooms,
    int TotalBookings,
    int ActiveBookings,
    int LongTermBookings,
    int TotalVehicles,
    int AvailableVehicles,
    int RentedVehicles,
    int ActiveVehicleBookings,
    decimal TotalIncomeThisMonth,
    decimal TotalExpensesThisMonth,
    decimal NetIncomeThisMonth,
    int PendingExpenseApprovals,
    int PendingPayrollApprovals,
    int PendingLeaveApprovals,
    int LowStockAgroItems);
