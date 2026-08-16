using Microsoft.EntityFrameworkCore;
using VendEstatesApp.Data;
using VendEstatesApp.Models;

namespace VendEstatesApp.Repositories;

public interface IBranchRepository : IGenericRepository<Branch>
{
    Task<IEnumerable<Branch>> GetActiveAsync();
}

public class BranchRepository : GenericRepository<Branch>, IBranchRepository
{
    public BranchRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Branch>> GetActiveAsync() =>
        await DbSet.AsNoTracking().Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync();
}
