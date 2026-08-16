using Microsoft.EntityFrameworkCore;
using VendEstatesApp.Data;
using VendEstatesApp.Models;

namespace VendEstatesApp.Repositories;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<IEnumerable<Notification>> GetByEmployeeAsync(int employeeId, int take = 10);

    Task<IEnumerable<Notification>> GetAllByEmployeeAsync(int employeeId);

    Task<int> GetUnreadCountAsync(int employeeId);

    Task MarkAllAsReadAsync(int employeeId);
}

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Notification>> GetByEmployeeAsync(int employeeId, int take = 10) =>
        await DbSet.AsNoTracking()
            .Where(n => n.EmployeeId == employeeId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync();

    public async Task<IEnumerable<Notification>> GetAllByEmployeeAsync(int employeeId) =>
        await DbSet.AsNoTracking()
            .Where(n => n.EmployeeId == employeeId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

    public async Task<int> GetUnreadCountAsync(int employeeId) =>
        await DbSet.CountAsync(n => n.EmployeeId == employeeId && !n.IsRead);

    public async Task MarkAllAsReadAsync(int employeeId)
    {
        var unread = await DbSet.Where(n => n.EmployeeId == employeeId && !n.IsRead).ToListAsync();
        foreach (var notification in unread)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await Context.SaveChangesAsync();
    }
}
