using VendEstatesApp.Models;
using VendEstatesApp.Repositories;

namespace VendEstatesApp.Services;

public interface IAuthService
{
    Task<Employee?> ValidateCredentialsAsync(string username, string password);

    string HashPassword(string password);

    bool VerifyPassword(string password, string passwordHash);
}

public class AuthService : IAuthService
{
    private readonly IEmployeeRepository _employeeRepository;

    public AuthService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<Employee?> ValidateCredentialsAsync(string username, string password)
    {
        var employee = await _employeeRepository.GetByUsernameAsync(username);
        if (employee is null || !employee.IsActive)
        {
            return null;
        }

        return VerifyPassword(password, employee.PasswordHash) ? employee : null;
    }

    public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool VerifyPassword(string password, string passwordHash) =>
        BCrypt.Net.BCrypt.Verify(password, passwordHash);
}
