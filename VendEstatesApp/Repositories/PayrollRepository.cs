using Microsoft.EntityFrameworkCore;
using VendEstatesApp.Data;
using VendEstatesApp.Models;
using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.Repositories;

public interface IPayrollRepository : IGenericRepository<Payroll>
{
    Task<IEnumerable<Payroll>> GetAllWithDetailsAsync();

    Task<Payroll?> GetByIdWithDetailsAsync(int id);

    Task<IEnumerable<Payroll>> GetByEmployeeAsync(int employeeId);

    Task<IEnumerable<Payroll>> GetByPeriodAsync(int month, int year);

    Task<IEnumerable<Payroll>> GetByStatusAsync(PayrollStatus status);

    Task<bool> ExistsForPeriodAsync(int employeeId, int month, int year);
}

public class PayrollRepository : GenericRepository<Payroll>, IPayrollRepository
{
    public PayrollRepository(ApplicationDbContext context) : base(context)
    {
    }

    private IQueryable<Payroll> WithDetails() =>
        DbSet.Include(p => p.Employee).ThenInclude(e => e!.Branch).Include(p => p.ApprovedByEmployee);

    public async Task<IEnumerable<Payroll>> GetAllWithDetailsAsync() =>
        await WithDetails().AsNoTracking().OrderByDescending(p => p.PayrollYear).ThenByDescending(p => p.PayrollMonth).ToListAsync();

    public async Task<Payroll?> GetByIdWithDetailsAsync(int id) =>
        await WithDetails().FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<Payroll>> GetByEmployeeAsync(int employeeId) =>
        await WithDetails().AsNoTracking().Where(p => p.EmployeeId == employeeId).ToListAsync();

    public async Task<IEnumerable<Payroll>> GetByPeriodAsync(int month, int year) =>
        await WithDetails().AsNoTracking().Where(p => p.PayrollMonth == month && p.PayrollYear == year).ToListAsync();

    public async Task<IEnumerable<Payroll>> GetByStatusAsync(PayrollStatus status) =>
        await WithDetails().AsNoTracking().Where(p => p.Status == status).ToListAsync();

    public async Task<bool> ExistsForPeriodAsync(int employeeId, int month, int year) =>
        await DbSet.AnyAsync(p => p.EmployeeId == employeeId && p.PayrollMonth == month && p.PayrollYear == year);
}
