using Microsoft.EntityFrameworkCore;
using VendEstatesApp.Data;
using VendEstatesApp.Models;

namespace VendEstatesApp.Repositories;

public interface IEmployeeRepository : IGenericRepository<Employee>
{
    Task<Employee?> GetByUsernameAsync(string username);

    Task<Employee?> GetByIdWithDetailsAsync(int id);

    Task<IEnumerable<Employee>> GetAllWithBranchAsync();

    Task<IEnumerable<Employee>> GetByBranchAsync(int branchId);

    Task<bool> UsernameExistsAsync(string username, int? excludeId = null);

    Task<bool> EmailExistsAsync(string? email, int? excludeId = null);
}

public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Employee?> GetByUsernameAsync(string username) =>
        await DbSet.FirstOrDefaultAsync(e => e.Username == username);

    public async Task<Employee?> GetByIdWithDetailsAsync(int id) =>
        await DbSet.Include(e => e.Branch).FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IEnumerable<Employee>> GetAllWithBranchAsync() =>
        await DbSet.Include(e => e.Branch).AsNoTracking().OrderBy(e => e.FirstName).ToListAsync();

    public async Task<IEnumerable<Employee>> GetByBranchAsync(int branchId) =>
        await DbSet.Include(e => e.Branch).AsNoTracking().Where(e => e.BranchId == branchId).ToListAsync();

    public async Task<bool> UsernameExistsAsync(string username, int? excludeId = null) =>
        await DbSet.AnyAsync(e => e.Username == username && (excludeId == null || e.Id != excludeId));

    public async Task<bool> EmailExistsAsync(string? email, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        return await DbSet.AnyAsync(e => e.Email == email && (excludeId == null || e.Id != excludeId));
    }
}
