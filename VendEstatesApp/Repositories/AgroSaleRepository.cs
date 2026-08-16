using Microsoft.EntityFrameworkCore;
using VendEstatesApp.Data;
using VendEstatesApp.Models;

namespace VendEstatesApp.Repositories;

public interface IAgroSaleRepository : IGenericRepository<AgroSale>
{
    Task<IEnumerable<AgroSale>> GetAllWithDetailsAsync();

    Task<IEnumerable<AgroSale>> GetByDateRangeAsync(DateTime start, DateTime end);
}

public class AgroSaleRepository : GenericRepository<AgroSale>, IAgroSaleRepository
{
    public AgroSaleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AgroSale>> GetAllWithDetailsAsync() =>
        await DbSet.Include(s => s.AgroInventory)
            .Include(s => s.SoldByEmployee)
            .AsNoTracking()
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();

    public async Task<IEnumerable<AgroSale>> GetByDateRangeAsync(DateTime start, DateTime end) =>
        await DbSet.Include(s => s.AgroInventory)
            .AsNoTracking()
            .Where(s => s.SaleDate >= start && s.SaleDate <= end)
            .ToListAsync();
}
