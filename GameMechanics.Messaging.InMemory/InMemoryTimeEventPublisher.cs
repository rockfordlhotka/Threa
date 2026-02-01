using Microsoft.Extensions.Logging;

namespace GameMechanics.Messaging.InMemory;

/// <summary>
/// In-memory implementation of <see cref="ITimeEventPublisher"/> using Rx.NET.
/// </summary>
public class InMemoryTimeEventPublisher : ITimeEventPublisher
{
    private readonly InMemoryMessageBus _bus;
    private readonly ILogger<InMemoryTimeEventPublisher> _logger;
    private bool _connected;
    private bool _disposed;

    public InMemoryTimeEventPublisher(
        InMemoryMessageBus bus,
        ILogger<InMemoryTimeEventPublisher> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public bool IsConnected => _connected && !_disposed;

    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(InMemoryTimeEventPublisher));

        _connected = true;
        _logger.LogInformation("In-memory time event publisher connected");
        return Task.CompletedTask;
    }

    public Task PublishTimeEventAsync(TimeEventMessage message, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        _bus.PublishTimeEvent(message);
        _logger.LogDebug("Published TimeEventMessage: {EventType} x {Count}", message.EventType, message.Count);

        return Task.CompletedTask;
    }

    public Task PublishTimeSkipAsync(TimeSkipMessage message, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        _bus.PublishTimeSkip(message);
        _logger.LogDebug("Published TimeSkipMessage: {SkipUnit} x {Count}", message.SkipUnit, message.Count);

        return Task.CompletedTask;
    }

    public Task PublishCombatStateAsync(CombatStateMessage message, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        _bus.PublishCombatState(message);
        _logger.LogDebug("Published CombatStateMessage: EnteringCombat={Entering}", message.EnteringCombat);

        return Task.CompletedTask;
    }

    public Task PublishCharacterUpdateAsync(CharacterUpdateMessage message, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        _bus.PublishCharacterUpdate(message);
        _logger.LogDebug("Published CharacterUpdateMessage: CharacterId={CharacterId}, Type={UpdateType}",
            message.CharacterId, message.UpdateType);

        return Task.CompletedTask;
    }

    public Task PublishTableUpdateAsync(TableUpdateMessage message, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        _bus.PublishTableUpdate(message);
        _logger.LogDebug("Published TableUpdateMessage: TableId={TableId}, Type={UpdateType}",
            message.TableId, message.UpdateType);

        return Task.CompletedTask;
    }

    public Task PublishJoinRequestAsync(JoinRequestMessage message, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        _bus.PublishJoinRequest(message);
        _logger.LogDebug("Published JoinRequestMessage: RequestId={RequestId}, Status={Status}",
            message.RequestId, message.Status);

        return Task.CompletedTask;
    }

    public Task PublishCharactersUpdatedAsync(CharactersUpdatedMessage message, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        _bus.PublishCharactersUpdated(message);
        _logger.LogDebug("Published CharactersUpdatedMessage: TableId={TableId}, CharacterCount={Count}, EventType={EventType}",
            message.TableId, message.CharacterIds.Count, message.EventType);

        return Task.CompletedTask;
    }

    public Task PublishTargetingRequestAsync(TargetingRequestMessage message, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        _bus.PublishTargetingRequest(message);
        _logger.LogDebug("Published TargetingRequestMessage: InteractionId={InteractionId}, Attacker={AttackerId}, Defender={DefenderId}",
            message.InteractionId, message.AttackerId, message.DefenderId);

        return Task.CompletedTask;
    }

    public Task PublishTargetingResponseAsync(TargetingResponseMessage message, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        _bus.PublishTargetingResponse(message);
        _logger.LogDebug("Published TargetingResponseMessage: InteractionId={InteractionId}, Acknowledged={Acknowledged}",
            message.InteractionId, message.Acknowledged);

        return Task.CompletedTask;
    }

    public Task PublishTargetingUpdateAsync(TargetingUpdateMessage message, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        _bus.PublishTargetingUpdate(message);
        _logger.LogDebug("Published TargetingUpdateMessage: InteractionId={InteractionId}, FromAttacker={IsFromAttacker}",
            message.InteractionId, message.IsFromAttacker);

        return Task.CompletedTask;
    }

    public Task PublishTargetingResultAsync(TargetingResultMessage message, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        _bus.PublishTargetingResult(message);
        _logger.LogDebug("Published TargetingResultMessage: InteractionId={InteractionId}, IsHit={IsHit}",
            message.InteractionId, message.Resolution.IsHit);

        return Task.CompletedTask;
    }

    public Task PublishTargetingCancelledAsync(TargetingCancelledMessage message, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        _bus.PublishTargetingCancelled(message);
        _logger.LogDebug("Published TargetingCancelledMessage: InteractionId={InteractionId}, AttackerId={AttackerId}",
            message.InteractionId, message.AttackerId);

        return Task.CompletedTask;
    }

    private void EnsureConnected()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(InMemoryTimeEventPublisher));

        if (!_connected)
            throw new InvalidOperationException("Publisher is not connected. Call ConnectAsync first.");
    }

    public ValueTask DisposeAsync()
    {
        if (_disposed) return ValueTask.CompletedTask;
        _disposed = true;
        _connected = false;

        _logger.LogInformation("In-memory time event publisher disposed");
        return ValueTask.CompletedTask;
    }
}
