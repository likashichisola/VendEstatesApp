using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using VendEstatesApp.Services;

namespace VendEstatesApp.ViewComponents;

/// <summary>
/// Renders the notification bell dropdown in the shared layout's top navbar.
/// </summary>
public class NotificationDropdownViewComponent : ViewComponent
{
    private readonly INotificationService _notificationService;

    public NotificationDropdownViewComponent(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var employeeIdClaim = UserClaimsPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(employeeIdClaim, out var employeeId))
        {
            return View(new NotificationDropdownViewModel([], 0));
        }

        var notifications = await _notificationService.GetRecentAsync(employeeId, 8);
        var unreadCount = await _notificationService.GetUnreadCountAsync(employeeId);

        return View(new NotificationDropdownViewModel(notifications.ToList(), unreadCount));
    }
}

public record NotificationDropdownViewModel(IReadOnlyList<Models.Notification> Notifications, int UnreadCount);
