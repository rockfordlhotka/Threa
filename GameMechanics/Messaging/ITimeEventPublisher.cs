using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameMechanics.Messaging;

/// <summary>
/// Publishes time events to a message broker.
/// Used by the GM to trigger time advancement across all connected clients.
/// </summary>
public interface ITimeEventPublisher : IAsyncDisposable
{
    /// <summary>
    /// Publishes a time event command to advance time.
    /// </summary>
    /// <param name="message">The time event command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishTimeEventAsync(TimeEventMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a time skip command for narrative time advancement.
    /// </summary>
    /// <param name="message">The time skip command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishTimeSkipAsync(TimeSkipMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a combat state change (enter/exit combat).
    /// </summary>
    /// <param name="message">The combat state message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishCombatStateAsync(CombatStateMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a character update notification.
    /// </summary>
    /// <param name="message">The character update message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishCharacterUpdateAsync(CharacterUpdateMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a table update notification (e.g., theme change).
    /// </summary>
    /// <param name="message">The table update message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishTableUpdateAsync(TableUpdateMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a join request notification (new request, approved, denied).
    /// </summary>
    /// <param name="message">The join request message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishJoinRequestAsync(JoinRequestMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Establishes connection to the message broker.
    /// </summary>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets whether the publisher is connected to the broker.
    /// </summary>
    bool IsConnected { get; }
}
