using Microsoft.EntityFrameworkCore;
using VendEstatesApp.Data;
using VendEstatesApp.Models;
using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.Repositories;

public interface IPaymentRepository : IGenericRepository<Payment>
{
    Task<IEnumerable<Payment>> GetAllWithDetailsAsync();

    Task<Payment?> GetByIdWithDetailsAsync(int id);

    Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status);

    Task<IEnumerable<Payment>> GetByTypeAsync(PaymentType type);
}

public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext context) : base(context)
    {
    }

    private IQueryable<Payment> WithDetails() =>
        DbSet.Include(p => p.Booking)
            .Include(p => p.VehicleBooking)
            .Include(p => p.Employee)
            .Include(p => p.Expense)
            .Include(p => p.Payroll)
            .Include(p => p.ProcessedByEmployee);

    public async Task<IEnumerable<Payment>> GetAllWithDetailsAsync() =>
        await WithDetails().AsNoTracking().OrderByDescending(p => p.PaymentDate).ToListAsync();

    public async Task<Payment?> GetByIdWithDetailsAsync(int id) =>
        await WithDetails().FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status) =>
        await WithDetails().AsNoTracking().Where(p => p.Status == status).ToListAsync();

    public async Task<IEnumerable<Payment>> GetByTypeAsync(PaymentType type) =>
        await WithDetails().AsNoTracking().Where(p => p.Type == type).ToListAsync();
}
