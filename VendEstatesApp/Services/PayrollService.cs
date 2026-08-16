using VendEstatesApp.Models;
using VendEstatesApp.Models.Enums;
using VendEstatesApp.Repositories;

namespace VendEstatesApp.Services;

public interface IPayrollService
{
    Task<IEnumerable<Payroll>> GetAllAsync();

    Task<Payroll?> GetByIdAsync(int id);

    Task<IEnumerable<Payroll>> GetByEmployeeAsync(int employeeId);

    Task<(bool Success, string? Error)> GeneratePayrollAsync(int employeeId, int month, int year, decimal allowances, decimal loanDeduction, decimal advanceDeduction, decimal otherDeductions, string? notes);

    Task<(bool Success, string? Error)> ApproveAsync(int payrollId, int approvedByEmployeeId);

    Task<(bool Success, string? Error)> RejectAsync(int payrollId, int rejectedByEmployeeId, string reason);

    Task<(bool Success, string? Error)> MarkPaidAsync(int payrollId);
}

/// <summary>
/// Payroll generation using <see cref="IPayrollCalculationService"/> plus the Director approval workflow.
/// </summary>
public class PayrollService : IPayrollService
{
    private readonly IPayrollRepository _payrollRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPayrollCalculationService _calculationService;
    private readonly INotificationService _notificationService;

    public PayrollService(
        IPayrollRepository payrollRepository,
        IEmployeeRepository employeeRepository,
        IPayrollCalculationService calculationService,
        INotificationService notificationService)
    {
        _payrollRepository = payrollRepository;
        _employeeRepository = employeeRepository;
        _calculationService = calculationService;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<Payroll>> GetAllAsync() => await _payrollRepository.GetAllWithDetailsAsync();

    public async Task<Payroll?> GetByIdAsync(int id) => await _payrollRepository.GetByIdWithDetailsAsync(id);

    public async Task<IEnumerable<Payroll>> GetByEmployeeAsync(int employeeId) => await _payrollRepository.GetByEmployeeAsync(employeeId);

    public async Task<(bool Success, string? Error)> GeneratePayrollAsync(int employeeId, int month, int year, decimal allowances, decimal loanDeduction, decimal advanceDeduction, decimal otherDeductions, string? notes)
    {
        if (await _payrollRepository.ExistsForPeriodAsync(employeeId, month, year))
        {
            return (false, "Payroll for this employee and period already exists.");
        }

        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee is null)
        {
            return (false, "Employee not found.");
        }

        var result = _calculationService.Calculate(employee.BasicSalary, allowances, loanDeduction, advanceDeduction, otherDeductions);

        var payroll = new Payroll
        {
            EmployeeId = employeeId,
            PayrollMonth = month,
            PayrollYear = year,
            Status = PayrollStatus.Pending,
            BasicSalary = employee.BasicSalary,
            Allowances = allowances,
            GrossSalary = result.GrossSalary,
            PayeTax = result.PayeTax,
            NapsaContribution = result.NapsaContribution,
            NhimaContribution = result.NhimaContribution,
            LoanDeduction = loanDeduction,
            AdvanceDeduction = advanceDeduction,
            OtherDeductions = otherDeductions,
            TotalDeductions = result.TotalDeductions,
            NetSalary = result.NetSalary,
            Notes = notes
        };

        await _payrollRepository.AddAsync(payroll);

        var directors = (await _employeeRepository.GetAllWithBranchAsync()).Where(e => e.Role == EmployeeRole.Director && e.IsActive);
        await _notificationService.NotifyRoleAsync(directors, NotificationType.Approval,
            "Payroll Awaiting Approval",
            $"Payroll for {employee.FullName} ({month}/{year}) needs your approval.",
            "/Payroll/Details/" + payroll.Id);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ApproveAsync(int payrollId, int approvedByEmployeeId)
    {
        var payroll = await _payrollRepository.GetByIdAsync(payrollId);
        if (payroll is null)
        {
            return (false, "Payroll record not found.");
        }

        payroll.Status = PayrollStatus.Approved;
        payroll.ApprovedByEmployeeId = approvedByEmployeeId;
        payroll.ApprovedAt = DateTime.UtcNow;
        await _payrollRepository.UpdateAsync(payroll);

        await _notificationService.NotifyAsync(payroll.EmployeeId, NotificationType.Payment,
            "Payroll Approved",
            $"Your payroll for {payroll.PayrollMonth}/{payroll.PayrollYear} has been approved.",
            "/Payroll/Details/" + payroll.Id);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> RejectAsync(int payrollId, int rejectedByEmployeeId, string reason)
    {
        var payroll = await _payrollRepository.GetByIdAsync(payrollId);
        if (payroll is null)
        {
            return (false, "Payroll record not found.");
        }

        payroll.Status = PayrollStatus.Rejected;
        payroll.ApprovedByEmployeeId = rejectedByEmployeeId;
        payroll.ApprovedAt = DateTime.UtcNow;
        payroll.Notes = reason;
        await _payrollRepository.UpdateAsync(payroll);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> MarkPaidAsync(int payrollId)
    {
        var payroll = await _payrollRepository.GetByIdAsync(payrollId);
        if (payroll is null)
        {
            return (false, "Payroll record not found.");
        }

        if (payroll.Status != PayrollStatus.Approved)
        {
            return (false, "Only approved payroll can be marked as paid.");
        }

        payroll.Status = PayrollStatus.Paid;
        payroll.PaidAt = DateTime.UtcNow;
        await _payrollRepository.UpdateAsync(payroll);

        await _notificationService.NotifyAsync(payroll.EmployeeId, NotificationType.Payment,
            "Salary Paid",
            $"Your net salary of {payroll.NetSalary:N2} for {payroll.PayrollMonth}/{payroll.PayrollYear} has been paid.",
            "/Payroll/Details/" + payroll.Id);

        return (true, null);
    }
}
