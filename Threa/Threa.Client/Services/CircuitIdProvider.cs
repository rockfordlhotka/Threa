namespace Threa.Client.Services;

/// <summary>
/// Provides the current circuit ID to components.
/// This is a scoped service that gets populated by the ActiveCircuitHandler.
/// </summary>
public class CircuitIdProvider
{
    public string CircuitId { get; set; } = string.Empty;
}
