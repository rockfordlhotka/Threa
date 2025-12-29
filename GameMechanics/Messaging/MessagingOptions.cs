namespace GameMechanics.Messaging;

/// <summary>
/// Configuration options for message broker connections.
/// Credentials (UserName, Password) should be configured via User Secrets or a secret store.
/// 
/// For development, use dotnet user-secrets:
///   dotnet user-secrets set "Messaging:UserName" "your-username"
///   dotnet user-secrets set "Messaging:Password" "your-password"
/// 
/// For production, use Azure Key Vault, AWS Secrets Manager, or environment variables:
///   Messaging__UserName=your-username
///   Messaging__Password=your-password
/// </summary>
public class MessagingOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Messaging";

    /// <summary>
    /// The message broker host name.
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// The message broker port.
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// The virtual host to use.
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Username for authentication.
    /// SECURITY: Configure via User Secrets or secret store, not appsettings.json.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Password for authentication.
    /// SECURITY: Configure via User Secrets or secret store, not appsettings.json.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Exchange name for time events.
    /// </summary>
    public string TimeEventExchange { get; set; } = "threa.time.events";

    /// <summary>
    /// Exchange name for time results.
    /// </summary>
    public string TimeResultExchange { get; set; } = "threa.time.results";

    /// <summary>
    /// Queue name for time event subscribers. If empty, a unique queue name will be generated.
    /// </summary>
    public string TimeEventQueue { get; set; } = "";

    /// <summary>
    /// Whether to use SSL/TLS.
    /// </summary>
    public bool UseSsl { get; set; } = false;

    /// <summary>
    /// Connection retry count.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Delay between connection retries in milliseconds.
    /// </summary>
    public int RetryDelayMs { get; set; } = 1000;

    /// <summary>
    /// Whether to automatically reconnect on connection loss.
    /// </summary>
    public bool AutoReconnect { get; set; } = true;

    /// <summary>
    /// Client-provided name for connection identification.
    /// </summary>
    public string? ClientName { get; set; }
}
