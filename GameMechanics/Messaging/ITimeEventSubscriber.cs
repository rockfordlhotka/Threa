using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameMechanics.Messaging;

/// <summary>
/// Subscribes to time events from a message broker.
/// Used by character assistants to receive time advancement commands.
/// </summary>
public interface ITimeEventSubscriber : IAsyncDisposable
{
    /// <summary>
    /// Event raised when a time event command is received.
    /// </summary>
    event EventHandler<TimeEventMessage>? TimeEventReceived;

    /// <summary>
    /// Event raised when a time skip command is received.
    /// </summary>
    event EventHandler<TimeSkipMessage>? TimeSkipReceived;

    /// <summary>
    /// Event raised when a combat state change is received.
    /// </summary>
    event EventHandler<CombatStateMessage>? CombatStateReceived;

    /// <summary>
    /// Event raised when a character update is received.
    /// </summary>
    event EventHandler<CharacterUpdateMessage>? CharacterUpdateReceived;

    /// <summary>
    /// Event raised when a table update is received (e.g., theme change).
    /// </summary>
    event EventHandler<TableUpdateMessage>? TableUpdateReceived;

    /// <summary>
    /// Establishes connection to the message broker.
    /// </summary>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts listening for time events.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SubscribeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops listening for time events.
    /// </summary>
    Task UnsubscribeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets whether the subscriber is actively listening.
    /// </summary>
    bool IsSubscribed { get; }

    /// <summary>
    /// Gets whether the subscriber is connected to the broker.
    /// </summary>
    bool IsConnected { get; }
}
