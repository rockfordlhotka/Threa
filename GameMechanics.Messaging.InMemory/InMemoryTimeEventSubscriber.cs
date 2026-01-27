using Microsoft.Extensions.Logging;

namespace GameMechanics.Messaging.InMemory;

/// <summary>
/// In-memory implementation of <see cref="ITimeEventSubscriber"/> using Rx.NET.
/// </summary>
public class InMemoryTimeEventSubscriber : ITimeEventSubscriber
{
    private readonly InMemoryMessageBus _bus;
    private readonly ILogger<InMemoryTimeEventSubscriber> _logger;

    private IDisposable? _timeEventSubscription;
    private IDisposable? _timeSkipSubscription;
    private IDisposable? _combatStateSubscription;
    private IDisposable? _characterUpdateSubscription;
    private IDisposable? _tableUpdateSubscription;
    private IDisposable? _joinRequestSubscription;

    private bool _connected;
    private bool _subscribed;
    private bool _disposed;

    public InMemoryTimeEventSubscriber(
        InMemoryMessageBus bus,
        ILogger<InMemoryTimeEventSubscriber> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public event EventHandler<TimeEventMessage>? TimeEventReceived;
    public event EventHandler<TimeSkipMessage>? TimeSkipReceived;
    public event EventHandler<CombatStateMessage>? CombatStateReceived;
    public event EventHandler<CharacterUpdateMessage>? CharacterUpdateReceived;
    public event EventHandler<TableUpdateMessage>? TableUpdateReceived;
    public event EventHandler<JoinRequestMessage>? JoinRequestReceived;

    public bool IsConnected => _connected && !_disposed;
    public bool IsSubscribed => _subscribed && !_disposed;

    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(InMemoryTimeEventSubscriber));

        _connected = true;
        _logger.LogInformation("In-memory time event subscriber connected");
        return Task.CompletedTask;
    }

    public Task SubscribeAsync(CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        if (_subscribed)
        {
            _logger.LogDebug("Already subscribed to time events");
            return Task.CompletedTask;
        }

        _timeEventSubscription = _bus.TimeEvents.Subscribe(msg =>
        {
            _logger.LogDebug("Received TimeEventMessage: {EventType} x {Count}", msg.EventType, msg.Count);
            TimeEventReceived?.Invoke(this, msg);
        });

        _timeSkipSubscription = _bus.TimeSkips.Subscribe(msg =>
        {
            _logger.LogDebug("Received TimeSkipMessage: {SkipUnit} x {Count}", msg.SkipUnit, msg.Count);
            TimeSkipReceived?.Invoke(this, msg);
        });

        _combatStateSubscription = _bus.CombatStates.Subscribe(msg =>
        {
            _logger.LogDebug("Received CombatStateMessage: EnteringCombat={Entering}", msg.EnteringCombat);
            CombatStateReceived?.Invoke(this, msg);
        });

        _characterUpdateSubscription = _bus.CharacterUpdates.Subscribe(msg =>
        {
            _logger.LogDebug("Received CharacterUpdateMessage: CharacterId={CharacterId}, Type={UpdateType}",
                msg.CharacterId, msg.UpdateType);
            CharacterUpdateReceived?.Invoke(this, msg);
        });

        _tableUpdateSubscription = _bus.TableUpdates.Subscribe(msg =>
        {
            _logger.LogDebug("Received TableUpdateMessage: TableId={TableId}, Type={UpdateType}",
                msg.TableId, msg.UpdateType);
            TableUpdateReceived?.Invoke(this, msg);
        });

        _joinRequestSubscription = _bus.JoinRequests.Subscribe(msg =>
        {
            _logger.LogDebug("Received JoinRequestMessage: RequestId={RequestId}, Status={Status}",
                msg.RequestId, msg.Status);
            JoinRequestReceived?.Invoke(this, msg);
        });

        _subscribed = true;
        _logger.LogInformation("Subscribed to in-memory time events");

        return Task.CompletedTask;
    }

    public Task UnsubscribeAsync(CancellationToken cancellationToken = default)
    {
        if (!_subscribed) return Task.CompletedTask;

        _timeEventSubscription?.Dispose();
        _timeSkipSubscription?.Dispose();
        _combatStateSubscription?.Dispose();
        _characterUpdateSubscription?.Dispose();
        _tableUpdateSubscription?.Dispose();
        _joinRequestSubscription?.Dispose();

        _timeEventSubscription = null;
        _timeSkipSubscription = null;
        _combatStateSubscription = null;
        _characterUpdateSubscription = null;
        _tableUpdateSubscription = null;
        _joinRequestSubscription = null;

        _subscribed = false;
        _logger.LogInformation("Unsubscribed from in-memory time events");

        return Task.CompletedTask;
    }

    private void EnsureConnected()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(InMemoryTimeEventSubscriber));

        if (!_connected)
            throw new InvalidOperationException("Subscriber is not connected. Call ConnectAsync first.");
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        await UnsubscribeAsync();
        _connected = false;

        _logger.LogInformation("In-memory time event subscriber disposed");
    }
}
