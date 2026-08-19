using VendEstatesApp.Models;
using VendEstatesApp.Repositories;

namespace VendEstatesApp.Services;

public interface IEmployeeService
{
    Task<IEnumerable<Employee>> GetAllAsync();

    Task<IEnumerable<Employee>> GetByBranchAsync(int branchId);

    Task<Employee?> GetByIdAsync(int id);

    Task<(bool Success, string? Error)> CreateAsync(Employee employee, string plainPassword);

    Task<(bool Success, string? Error)> UpdateAsync(Employee employee);

    Task<(bool Success, string? Error)> ChangePasswordAsync(int employeeId, string newPassword);

    Task DeactivateAsync(int id);

    Task DeleteAsync(int id);

    Task<IEnumerable<Employee>> GetDirectorsAsync();
}

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAuthService _authService;

    public EmployeeService(IEmployeeRepository employeeRepository, IAuthService authService)
    {
        _employeeRepository = employeeRepository;
        _authService = authService;
    }

    public async Task<IEnumerable<Employee>> GetAllAsync() => await _employeeRepository.GetAllWithBranchAsync();

    public async Task<IEnumerable<Employee>> GetByBranchAsync(int branchId) => await _employeeRepository.GetByBranchAsync(branchId);

    public async Task<Employee?> GetByIdAsync(int id) => await _employeeRepository.GetByIdWithDetailsAsync(id);

    public async Task<(bool Success, string? Error)> CreateAsync(Employee employee, string plainPassword)
    {
        if (await _employeeRepository.UsernameExistsAsync(employee.Username))
        {
            return (false, "Username already exists.");
        }

        if (await _employeeRepository.EmailExistsAsync(employee.Email))
        {
            return (false, "Email already exists.");
        }

        employee.PasswordHash = _authService.HashPassword(plainPassword);
        await _employeeRepository.AddAsync(employee);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(Employee employee)
    {
        if (await _employeeRepository.UsernameExistsAsync(employee.Username, employee.Id))
        {
            return (false, "Username already exists.");
        }

        if (await _employeeRepository.EmailExistsAsync(employee.Email, employee.Id))
        {
            return (false, "Email already exists.");
        }

        var existing = await _employeeRepository.GetByIdAsync(employee.Id);
        if (existing is null)
        {
            return (false, "Employee not found.");
        }

        existing.FirstName = employee.FirstName;
        existing.LastName = employee.LastName;
        existing.Username = employee.Username;
        existing.Email = employee.Email;
        existing.PhoneNumber = employee.PhoneNumber;
        existing.Role = employee.Role;
        existing.JobTitle = employee.JobTitle;
        existing.BranchId = employee.BranchId;
        existing.BasicSalary = employee.BasicSalary;
        existing.IsActive = employee.IsActive;
        existing.NapsaNumber = employee.NapsaNumber;
        existing.NhimaNumber = employee.NhimaNumber;
        existing.TpinNumber = employee.TpinNumber;
        existing.ZraNumber = employee.ZraNumber;

        await _employeeRepository.UpdateAsync(existing);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(int employeeId, string newPassword)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee is null)
        {
            return (false, "Employee not found.");
        }

        employee.PasswordHash = _authService.HashPassword(newPassword);
        await _employeeRepository.UpdateAsync(employee);
        return (true, null);
    }

    public async Task DeactivateAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        if (employee is not null)
        {
            employee.IsActive = false;
            await _employeeRepository.UpdateAsync(employee);
        }
    }

    public async Task DeleteAsync(int id) => await _employeeRepository.DeleteAsync(id);

    public async Task<IEnumerable<Employee>> GetDirectorsAsync()
    {
        var all = await _employeeRepository.GetAllWithBranchAsync();
        return all.Where(e => e.Role == Models.Enums.EmployeeRole.Director && e.IsActive);
    }
}
