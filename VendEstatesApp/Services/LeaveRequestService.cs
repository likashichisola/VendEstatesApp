using VendEstatesApp.Models;
using VendEstatesApp.Models.Enums;
using VendEstatesApp.Repositories;

namespace VendEstatesApp.Services;

public interface ILeaveRequestService
{
    Task<IEnumerable<LeaveRequest>> GetAllAsync();

    Task<LeaveRequest?> GetByIdAsync(int id);

    Task<IEnumerable<LeaveRequest>> GetByEmployeeAsync(int employeeId);

    Task<IEnumerable<LeaveRequest>> GetPendingAsync();

    Task<(bool Success, string? Error)> ApplyAsync(LeaveRequest leaveRequest);

    Task<(bool Success, string? Error)> ApproveAsync(int leaveRequestId, int approvedByEmployeeId, string? notes);

    Task<(bool Success, string? Error)> RejectAsync(int leaveRequestId, int rejectedByEmployeeId, string? notes);

    Task<(bool Success, string? Error)> CancelAsync(int leaveRequestId);

    Task<int> GetUsedLeaveDaysAsync(int employeeId, int year);
}

public class LeaveRequestService : ILeaveRequestService
{
    private const int AnnualLeaveEntitlementDays = 24;

    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly INotificationService _notificationService;

    public LeaveRequestService(
        ILeaveRequestRepository leaveRequestRepository,
        IEmployeeRepository employeeRepository,
        INotificationService notificationService)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _employeeRepository = employeeRepository;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<LeaveRequest>> GetAllAsync() => await _leaveRequestRepository.GetAllWithDetailsAsync();

    public async Task<LeaveRequest?> GetByIdAsync(int id) => await _leaveRequestRepository.GetByIdWithDetailsAsync(id);

    public async Task<IEnumerable<LeaveRequest>> GetByEmployeeAsync(int employeeId) => await _leaveRequestRepository.GetByEmployeeAsync(employeeId);

    public async Task<IEnumerable<LeaveRequest>> GetPendingAsync() => await _leaveRequestRepository.GetByStatusAsync(LeaveStatus.Pending);

    public async Task<(bool Success, string? Error)> ApplyAsync(LeaveRequest leaveRequest)
    {
        if (leaveRequest.EndDate < leaveRequest.StartDate)
        {
            return (false, "End date must be on or after the start date.");
        }

        leaveRequest.Status = LeaveStatus.Pending;
        await _leaveRequestRepository.AddAsync(leaveRequest);

        var employee = await _employeeRepository.GetByIdAsync(leaveRequest.EmployeeId);
        var directorsAndManagers = (await _employeeRepository.GetAllWithBranchAsync())
            .Where(e => (e.Role == EmployeeRole.Director || e.Role == EmployeeRole.Manager) && e.IsActive);

        await _notificationService.NotifyRoleAsync(directorsAndManagers, NotificationType.Leave,
            "Leave Request Submitted",
            $"{employee?.FullName ?? "An employee"} applied for {leaveRequest.Type} leave ({leaveRequest.NumberOfDays} day(s)).",
            "/Leave/Details/" + leaveRequest.Id);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ApproveAsync(int leaveRequestId, int approvedByEmployeeId, string? notes)
    {
        var leaveRequest = await _leaveRequestRepository.GetByIdAsync(leaveRequestId);
        if (leaveRequest is null)
        {
            return (false, "Leave request not found.");
        }

        leaveRequest.Status = LeaveStatus.Approved;
        leaveRequest.ApprovedByEmployeeId = approvedByEmployeeId;
        leaveRequest.DecisionAt = DateTime.UtcNow;
        leaveRequest.DecisionNotes = notes;
        await _leaveRequestRepository.UpdateAsync(leaveRequest);

        await _notificationService.NotifyAsync(leaveRequest.EmployeeId, NotificationType.Leave,
            "Leave Request Approved",
            $"Your {leaveRequest.Type} leave request has been approved.",
            "/Leave/Details/" + leaveRequest.Id);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> RejectAsync(int leaveRequestId, int rejectedByEmployeeId, string? notes)
    {
        var leaveRequest = await _leaveRequestRepository.GetByIdAsync(leaveRequestId);
        if (leaveRequest is null)
        {
            return (false, "Leave request not found.");
        }

        leaveRequest.Status = LeaveStatus.Rejected;
        leaveRequest.ApprovedByEmployeeId = rejectedByEmployeeId;
        leaveRequest.DecisionAt = DateTime.UtcNow;
        leaveRequest.DecisionNotes = notes;
        await _leaveRequestRepository.UpdateAsync(leaveRequest);

        await _notificationService.NotifyAsync(leaveRequest.EmployeeId, NotificationType.Leave,
            "Leave Request Rejected",
            $"Your {leaveRequest.Type} leave request was rejected.",
            "/Leave/Details/" + leaveRequest.Id);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> CancelAsync(int leaveRequestId)
    {
        var leaveRequest = await _leaveRequestRepository.GetByIdAsync(leaveRequestId);
        if (leaveRequest is null)
        {
            return (false, "Leave request not found.");
        }

        leaveRequest.Status = LeaveStatus.Cancelled;
        await _leaveRequestRepository.UpdateAsync(leaveRequest);
        return (true, null);
    }

    public async Task<int> GetUsedLeaveDaysAsync(int employeeId, int year)
    {
        var requests = await _leaveRequestRepository.GetByEmployeeAsync(employeeId);
        return requests
            .Where(l => l.Status == LeaveStatus.Approved && l.StartDate.Year == year)
            .Sum(l => l.NumberOfDays);
    }
}
