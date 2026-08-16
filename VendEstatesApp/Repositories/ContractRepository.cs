using Microsoft.EntityFrameworkCore;
using VendEstatesApp.Data;
using VendEstatesApp.Models;

namespace VendEstatesApp.Repositories;

public interface IContractRepository : IGenericRepository<Contract>
{
    Task<IEnumerable<Contract>> GetAllWithEmployeeAsync();

    Task<Contract?> GetByIdWithEmployeeAsync(int id);

    Task<IEnumerable<Contract>> GetByEmployeeAsync(int employeeId);
}

public class ContractRepository : GenericRepository<Contract>, IContractRepository
{
    public ContractRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Contract>> GetAllWithEmployeeAsync() =>
        await DbSet.Include(c => c.Employee)
            .AsNoTracking()
            .OrderByDescending(c => c.StartDate)
            .ToListAsync();

    public async Task<Contract?> GetByIdWithEmployeeAsync(int id) =>
        await DbSet.Include(c => c.Employee).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IEnumerable<Contract>> GetByEmployeeAsync(int employeeId) =>
        await DbSet.AsNoTracking().Where(c => c.EmployeeId == employeeId).ToListAsync();
}
