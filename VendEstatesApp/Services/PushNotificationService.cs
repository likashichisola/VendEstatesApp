using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VendEstatesApp.Models;
using VendEstatesApp.Repositories;
using WebPush;

namespace VendEstatesApp.Services;

public interface IPushNotificationService
{
    string GetPublicKey();

    Task<(bool Success, string? Error)> SubscribeAsync(int employeeId, string endpoint, string p256dhKey, string authKey, string? userAgent);

    Task UnsubscribeAsync(string endpoint);

    Task SendToEmployeeAsync(int employeeId, string title, string message, string? linkUrl = null);
}

/// <summary>
/// Sends Web Push notifications to subscribed browsers using VAPID-signed requests.
/// Failures (expired/invalid subscriptions) are handled by removing the stale subscription
/// and never bubble up to callers, so in-app notifications always succeed regardless of push delivery.
/// </summary>
public class PushNotificationService : IPushNotificationService
{
    private readonly IPushSubscriptionRepository _subscriptionRepository;
    private readonly VapidSettings _vapidSettings;
    private readonly WebPushClient _webPushClient;
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(
        IPushSubscriptionRepository subscriptionRepository,
        IOptions<VapidSettings> vapidSettings,
        ILogger<PushNotificationService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _vapidSettings = vapidSettings.Value;
        _webPushClient = new WebPushClient();
        _logger = logger;
    }

    public string GetPublicKey() => _vapidSettings.PublicKey;

    public async Task<(bool Success, string? Error)> SubscribeAsync(int employeeId, string endpoint, string p256dhKey, string authKey, string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(p256dhKey) || string.IsNullOrWhiteSpace(authKey))
        {
            return (false, "Invalid subscription payload.");
        }

        var existing = await _subscriptionRepository.GetByEndpointAsync(endpoint);
        if (existing is not null)
        {
            existing.EmployeeId = employeeId;
            existing.P256dhKey = p256dhKey;
            existing.AuthKey = authKey;
            existing.UserAgent = userAgent;
            existing.LastUsedAt = DateTime.UtcNow;
            await _subscriptionRepository.UpdateAsync(existing);
            return (true, null);
        }

        await _subscriptionRepository.AddAsync(new Models.PushSubscription
        {
            EmployeeId = employeeId,
            Endpoint = endpoint,
            P256dhKey = p256dhKey,
            AuthKey = authKey,
            UserAgent = userAgent,
            LastUsedAt = DateTime.UtcNow
        });

        return (true, null);
    }

    public Task UnsubscribeAsync(string endpoint) =>
        _subscriptionRepository.RemoveByEndpointAsync(endpoint);

    public async Task SendToEmployeeAsync(int employeeId, string title, string message, string? linkUrl = null)
    {
        if (string.IsNullOrWhiteSpace(_vapidSettings.PublicKey) || string.IsNullOrWhiteSpace(_vapidSettings.PrivateKey))
        {
            // VAPID keys not configured; skip push delivery silently (in-app notifications remain unaffected).
            return;
        }

        var subscriptions = await _subscriptionRepository.GetByEmployeeAsync(employeeId);
        var payload = System.Text.Json.JsonSerializer.Serialize(new { title, message, url = linkUrl });

        foreach (var subscription in subscriptions)
        {
            var pushSubscription = new WebPush.PushSubscription(subscription.Endpoint, subscription.P256dhKey, subscription.AuthKey);
            var vapidDetails = new VapidDetails(_vapidSettings.Subject, _vapidSettings.PublicKey, _vapidSettings.PrivateKey);

            try
            {
                await _webPushClient.SendNotificationAsync(pushSubscription, payload, vapidDetails);
            }
            catch (WebPushException ex) when (ex.StatusCode is System.Net.HttpStatusCode.Gone or System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Removing stale push subscription for employee {EmployeeId}: {Reason}", employeeId, ex.Message);
                await _subscriptionRepository.RemoveByEndpointAsync(subscription.Endpoint);
            }
            catch (WebPushException ex)
            {
                _logger.LogWarning(ex, "Failed to send push notification to employee {EmployeeId}", employeeId);
            }
        }
    }
}
