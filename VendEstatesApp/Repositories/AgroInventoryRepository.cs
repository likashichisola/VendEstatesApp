using Microsoft.EntityFrameworkCore;
using VendEstatesApp.Data;
using VendEstatesApp.Models;

namespace VendEstatesApp.Repositories;

public interface IAgroInventoryRepository : IGenericRepository<AgroInventory>
{
    Task<IEnumerable<AgroInventory>> GetAllWithBranchAsync();

    Task<AgroInventory?> GetByIdWithBranchAsync(int id);

    Task<IEnumerable<AgroInventory>> GetLowStockAsync();
}

public class AgroInventoryRepository : GenericRepository<AgroInventory>, IAgroInventoryRepository
{
    public AgroInventoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AgroInventory>> GetAllWithBranchAsync() =>
        await DbSet.Include(a => a.Branch).AsNoTracking().OrderBy(a => a.ItemName).ToListAsync();

    public async Task<AgroInventory?> GetByIdWithBranchAsync(int id) =>
        await DbSet.Include(a => a.Branch).FirstOrDefaultAsync(a => a.Id == id);

    public async Task<IEnumerable<AgroInventory>> GetLowStockAsync() =>
        await DbSet.Include(a => a.Branch)
            .AsNoTracking()
            .Where(a => a.QuantityInStock <= a.ReorderLevel)
            .ToListAsync();
}
