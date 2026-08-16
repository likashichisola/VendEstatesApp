using Microsoft.EntityFrameworkCore;
using VendEstatesApp.Data;
using VendEstatesApp.Models;

namespace VendEstatesApp.Repositories;

public interface IPushSubscriptionRepository : IGenericRepository<PushSubscription>
{
    Task<IEnumerable<PushSubscription>> GetByEmployeeAsync(int employeeId);

    Task<PushSubscription?> GetByEndpointAsync(string endpoint);

    Task RemoveByEndpointAsync(string endpoint);
}

public class PushSubscriptionRepository : GenericRepository<PushSubscription>, IPushSubscriptionRepository
{
    public PushSubscriptionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PushSubscription>> GetByEmployeeAsync(int employeeId) =>
        await DbSet.AsNoTracking()
            .Where(p => p.EmployeeId == employeeId)
            .ToListAsync();

    public async Task<PushSubscription?> GetByEndpointAsync(string endpoint) =>
        await DbSet.FirstOrDefaultAsync(p => p.Endpoint == endpoint);

    public async Task RemoveByEndpointAsync(string endpoint)
    {
        var subscription = await DbSet.FirstOrDefaultAsync(p => p.Endpoint == endpoint);
        if (subscription is not null)
        {
            DbSet.Remove(subscription);
            await Context.SaveChangesAsync();
        }
    }
}
