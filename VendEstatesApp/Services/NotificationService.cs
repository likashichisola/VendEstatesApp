using VendEstatesApp.Models;
using VendEstatesApp.Models.Enums;
using VendEstatesApp.Repositories;

namespace VendEstatesApp.Services;

public interface INotificationService
{
    Task<IEnumerable<Notification>> GetRecentAsync(int employeeId, int take = 10);

    Task<IEnumerable<Notification>> GetAllForEmployeeAsync(int employeeId);

    Task<int> GetUnreadCountAsync(int employeeId);

    Task MarkAllAsReadAsync(int employeeId);

    Task<(bool Success, string? Error)> MarkAsReadAsync(int notificationId, int employeeId);

    Task NotifyAsync(int employeeId, NotificationType type, string title, string message, string? linkUrl = null);

    Task NotifyRoleAsync(IEnumerable<Employee> recipients, NotificationType type, string title, string message, string? linkUrl = null);
}

/// <summary>
/// Raises in-app notifications: directors on approvals, employees for payments, managers on booking updates.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IPushNotificationService _pushNotificationService;

    public NotificationService(INotificationRepository notificationRepository, IPushNotificationService pushNotificationService)
    {
        _notificationRepository = notificationRepository;
        _pushNotificationService = pushNotificationService;
    }

    public async Task<IEnumerable<Notification>> GetRecentAsync(int employeeId, int take = 10) =>
        await _notificationRepository.GetByEmployeeAsync(employeeId, take);

    public async Task<IEnumerable<Notification>> GetAllForEmployeeAsync(int employeeId) =>
        await _notificationRepository.GetAllByEmployeeAsync(employeeId);

    public async Task<int> GetUnreadCountAsync(int employeeId) =>
        await _notificationRepository.GetUnreadCountAsync(employeeId);

    public async Task MarkAllAsReadAsync(int employeeId) =>
        await _notificationRepository.MarkAllAsReadAsync(employeeId);

    public async Task<(bool Success, string? Error)> MarkAsReadAsync(int notificationId, int employeeId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        if (notification is null || notification.EmployeeId != employeeId)
        {
            return (false, "Notification not found.");
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _notificationRepository.UpdateAsync(notification);
        }

        return (true, null);
    }

    public async Task NotifyAsync(int employeeId, NotificationType type, string title, string message, string? linkUrl = null)
    {
        var notification = new Notification
        {
            EmployeeId = employeeId,
            Type = type,
            Title = title,
            Message = message,
            LinkUrl = linkUrl
        };

        await _notificationRepository.AddAsync(notification);

        await _pushNotificationService.SendToEmployeeAsync(employeeId, title, message, linkUrl);
    }

    public async Task NotifyRoleAsync(IEnumerable<Employee> recipients, NotificationType type, string title, string message, string? linkUrl = null)
    {
        foreach (var recipient in recipients)
        {
            await NotifyAsync(recipient.Id, type, title, message, linkUrl);
        }
    }
}
