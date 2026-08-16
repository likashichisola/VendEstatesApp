using Microsoft.EntityFrameworkCore;
using VendEstatesApp.Data;
using VendEstatesApp.Models;
using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.Repositories;

public interface ILeaveRequestRepository : IGenericRepository<LeaveRequest>
{
    Task<IEnumerable<LeaveRequest>> GetAllWithDetailsAsync();

    Task<LeaveRequest?> GetByIdWithDetailsAsync(int id);

    Task<IEnumerable<LeaveRequest>> GetByEmployeeAsync(int employeeId);

    Task<IEnumerable<LeaveRequest>> GetByStatusAsync(LeaveStatus status);
}

public class LeaveRequestRepository : GenericRepository<LeaveRequest>, ILeaveRequestRepository
{
    public LeaveRequestRepository(ApplicationDbContext context) : base(context)
    {
    }

    private IQueryable<LeaveRequest> WithDetails() =>
        DbSet.Include(l => l.Employee).Include(l => l.ApprovedByEmployee);

    public async Task<IEnumerable<LeaveRequest>> GetAllWithDetailsAsync() =>
        await WithDetails().AsNoTracking().OrderByDescending(l => l.StartDate).ToListAsync();

    public async Task<LeaveRequest?> GetByIdWithDetailsAsync(int id) =>
        await WithDetails().FirstOrDefaultAsync(l => l.Id == id);

    public async Task<IEnumerable<LeaveRequest>> GetByEmployeeAsync(int employeeId) =>
        await WithDetails().AsNoTracking().Where(l => l.EmployeeId == employeeId).ToListAsync();

    public async Task<IEnumerable<LeaveRequest>> GetByStatusAsync(LeaveStatus status) =>
        await WithDetails().AsNoTracking().Where(l => l.Status == status).ToListAsync();
}
