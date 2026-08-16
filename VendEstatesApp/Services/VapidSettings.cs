namespace VendEstatesApp.Services;

/// <summary>
/// VAPID (Voluntary Application Server Identification) configuration used to sign Web Push requests.
/// Bound from the "Vapid" section of configuration.
/// </summary>
public class VapidSettings
{
    public string PublicKey { get; set; } = string.Empty;

    public string PrivateKey { get; set; } = string.Empty;

    public string Subject { get; set; } = "mailto:admin@vendestates.local";
}
