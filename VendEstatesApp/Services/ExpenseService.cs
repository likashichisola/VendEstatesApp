using VendEstatesApp.Models;
using VendEstatesApp.Models.Enums;
using VendEstatesApp.Repositories;

namespace VendEstatesApp.Services;

public interface IExpenseService
{
    Task<IEnumerable<Expense>> GetAllAsync();

    Task<Expense?> GetByIdAsync(int id);

    Task<IEnumerable<Expense>> GetByBranchAsync(int branchId);

    Task<IEnumerable<Expense>> GetByStatusAsync(ExpenseStatus status);

    Task<Expense> SubmitAsync(Expense expense);

    Task<(bool Success, string? Error)> ApproveAsync(int expenseId, int approvedByEmployeeId);

    Task<(bool Success, string? Error)> RejectAsync(int expenseId, int rejectedByEmployeeId, string reason);

    Task<(bool Success, string? Error)> MarkPaidAsync(int expenseId);

    Task DeleteAsync(int id);
}

/// <summary>
/// Expense request submission plus the Director approval workflow.
/// </summary>
public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly INotificationService _notificationService;

    public ExpenseService(
        IExpenseRepository expenseRepository,
        IEmployeeRepository employeeRepository,
        INotificationService notificationService)
    {
        _expenseRepository = expenseRepository;
        _employeeRepository = employeeRepository;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<Expense>> GetAllAsync() => await _expenseRepository.GetAllWithDetailsAsync();

    public async Task<Expense?> GetByIdAsync(int id) => await _expenseRepository.GetByIdWithDetailsAsync(id);

    public async Task<IEnumerable<Expense>> GetByBranchAsync(int branchId) => await _expenseRepository.GetByBranchAsync(branchId);

    public async Task<IEnumerable<Expense>> GetByStatusAsync(ExpenseStatus status) => await _expenseRepository.GetByStatusAsync(status);

    public async Task<Expense> SubmitAsync(Expense expense)
    {
        expense.Status = ExpenseStatus.Pending;
        await _expenseRepository.AddAsync(expense);

        var requester = await _employeeRepository.GetByIdAsync(expense.RequestedByEmployeeId);
        var directors = (await _employeeRepository.GetAllWithBranchAsync()).Where(e => e.Role == EmployeeRole.Director && e.IsActive);
        await _notificationService.NotifyRoleAsync(directors, NotificationType.Approval,
            "Expense Awaiting Approval",
            $"{requester?.FullName ?? "An employee"} submitted an expense request: {expense.Title} ({expense.Amount:N2}).",
            "/Expense/Details/" + expense.Id);

        return expense;
    }

    public async Task<(bool Success, string? Error)> ApproveAsync(int expenseId, int approvedByEmployeeId)
    {
        var expense = await _expenseRepository.GetByIdAsync(expenseId);
        if (expense is null)
        {
            return (false, "Expense not found.");
        }

        expense.Status = ExpenseStatus.Approved;
        expense.ApprovedByEmployeeId = approvedByEmployeeId;
        expense.ApprovedAt = DateTime.UtcNow;
        await _expenseRepository.UpdateAsync(expense);

        await _notificationService.NotifyAsync(expense.RequestedByEmployeeId, NotificationType.Approval,
            "Expense Approved",
            $"Your expense request '{expense.Title}' has been approved.",
            "/Expense/Details/" + expense.Id);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> RejectAsync(int expenseId, int rejectedByEmployeeId, string reason)
    {
        var expense = await _expenseRepository.GetByIdAsync(expenseId);
        if (expense is null)
        {
            return (false, "Expense not found.");
        }

        expense.Status = ExpenseStatus.Rejected;
        expense.ApprovedByEmployeeId = rejectedByEmployeeId;
        expense.ApprovedAt = DateTime.UtcNow;
        expense.RejectionReason = reason;
        await _expenseRepository.UpdateAsync(expense);

        await _notificationService.NotifyAsync(expense.RequestedByEmployeeId, NotificationType.Approval,
            "Expense Rejected",
            $"Your expense request '{expense.Title}' was rejected: {reason}",
            "/Expense/Details/" + expense.Id);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> MarkPaidAsync(int expenseId)
    {
        var expense = await _expenseRepository.GetByIdAsync(expenseId);
        if (expense is null)
        {
            return (false, "Expense not found.");
        }

        if (expense.Status != ExpenseStatus.Approved)
        {
            return (false, "Only approved expenses can be marked as paid.");
        }

        expense.Status = ExpenseStatus.Paid;
        await _expenseRepository.UpdateAsync(expense);
        return (true, null);
    }

    public async Task DeleteAsync(int id) => await _expenseRepository.DeleteAsync(id);
}
