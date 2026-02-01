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
    /// Event raised when a join request is received.
    /// </summary>
    event EventHandler<JoinRequestMessage>? JoinRequestReceived;

    /// <summary>
    /// Event raised when characters have been updated after time processing.
    /// Player clients should refresh their character state from the database.
    /// </summary>
    event EventHandler<CharactersUpdatedMessage>? CharactersUpdatedReceived;

    /// <summary>
    /// Event raised when a targeting request is received (attacker initiates).
    /// </summary>
    event EventHandler<TargetingRequestMessage>? TargetingRequestReceived;

    /// <summary>
    /// Event raised when a targeting response is received (defender acknowledges).
    /// </summary>
    event EventHandler<TargetingResponseMessage>? TargetingResponseReceived;

    /// <summary>
    /// Event raised when a targeting update is received (either party updates data).
    /// </summary>
    event EventHandler<TargetingUpdateMessage>? TargetingUpdateReceived;

    /// <summary>
    /// Event raised when a targeting result is received (attack resolved).
    /// </summary>
    event EventHandler<TargetingResultMessage>? TargetingResultReceived;

    /// <summary>
    /// Event raised when targeting is cancelled.
    /// </summary>
    event EventHandler<TargetingCancelledMessage>? TargetingCancelledReceived;

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
