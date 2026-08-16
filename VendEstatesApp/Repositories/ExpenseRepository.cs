using Microsoft.EntityFrameworkCore;
using VendEstatesApp.Data;
using VendEstatesApp.Models;
using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.Repositories;

public interface IExpenseRepository : IGenericRepository<Expense>
{
    Task<IEnumerable<Expense>> GetAllWithDetailsAsync();

    Task<Expense?> GetByIdWithDetailsAsync(int id);

    Task<IEnumerable<Expense>> GetByStatusAsync(ExpenseStatus status);

    Task<IEnumerable<Expense>> GetByBranchAsync(int branchId);
}

public class ExpenseRepository : GenericRepository<Expense>, IExpenseRepository
{
    public ExpenseRepository(ApplicationDbContext context) : base(context)
    {
    }

    private IQueryable<Expense> WithDetails() =>
        DbSet.Include(e => e.Branch)
            .Include(e => e.RequestedByEmployee)
            .Include(e => e.ApprovedByEmployee);

    public async Task<IEnumerable<Expense>> GetAllWithDetailsAsync() =>
        await WithDetails().AsNoTracking().OrderByDescending(e => e.ExpenseDate).ToListAsync();

    public async Task<Expense?> GetByIdWithDetailsAsync(int id) =>
        await WithDetails().FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IEnumerable<Expense>> GetByStatusAsync(ExpenseStatus status) =>
        await WithDetails().AsNoTracking().Where(e => e.Status == status).ToListAsync();

    public async Task<IEnumerable<Expense>> GetByBranchAsync(int branchId) =>
        await WithDetails().AsNoTracking().Where(e => e.BranchId == branchId).ToListAsync();
}
