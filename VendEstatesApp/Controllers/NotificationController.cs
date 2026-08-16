using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendEstatesApp.Constants;
using VendEstatesApp.Services;

namespace VendEstatesApp.Controllers;

[Authorize(Roles = Roles.All)]
public class NotificationController : Controller
{
    private readonly INotificationService _notificationService;
    private readonly IPushNotificationService _pushNotificationService;

    public NotificationController(INotificationService notificationService, IPushNotificationService pushNotificationService)
    {
        _notificationService = notificationService;
        _pushNotificationService = pushNotificationService;
    }

    public async Task<IActionResult> Index()
    {
        var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var notifications = await _notificationService.GetAllForEmployeeAsync(employeeId);
        return View(notifications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _notificationService.MarkAllAsReadAsync(employeeId);

        var returnUrl = Request.Headers.Referer.ToString();
        return string.IsNullOrEmpty(returnUrl) ? RedirectToAction(nameof(Index)) : Redirect(returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(int id, string? returnUrl)
    {
        var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _notificationService.MarkAsReadAsync(id, employeeId);

        return string.IsNullOrEmpty(returnUrl) ? RedirectToAction(nameof(Index)) : Redirect(returnUrl);
    }

    [HttpGet]
    public IActionResult VapidPublicKey() => Json(new { publicKey = _pushNotificationService.GetPublicKey() });

    public class PushSubscriptionDto
    {
        public string Endpoint { get; set; } = string.Empty;

        public PushKeysDto Keys { get; set; } = new();
    }

    public class PushKeysDto
    {
        public string P256dh { get; set; } = string.Empty;

        public string Auth { get; set; } = string.Empty;
    }

    [HttpPost]
    public async Task<IActionResult> Subscribe([FromBody] PushSubscriptionDto subscription)
    {
        var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userAgent = Request.Headers.UserAgent.ToString();

        var (success, error) = await _pushNotificationService.SubscribeAsync(
            employeeId, subscription.Endpoint, subscription.Keys.P256dh, subscription.Keys.Auth, userAgent);

        return success ? Ok() : BadRequest(new { error });
    }

    [HttpPost]
    public async Task<IActionResult> Unsubscribe([FromBody] PushSubscriptionDto subscription)
    {
        await _pushNotificationService.UnsubscribeAsync(subscription.Endpoint);
        return Ok();
    }
}
