using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace GameMechanics.Messaging.InMemory;

/// <summary>
/// In-memory message bus using Rx.NET for pub/sub messaging.
/// This is a singleton service that allows components to publish and subscribe to messages.
/// </summary>
public class InMemoryMessageBus : IDisposable
{
    private readonly Subject<TimeEventMessage> _timeEvents = new();
    private readonly Subject<TimeSkipMessage> _timeSkips = new();
    private readonly Subject<CombatStateMessage> _combatStates = new();
    private readonly Subject<TimeResultMessage> _timeResults = new();
    private readonly Subject<ActivityLogMessage> _activityLogs = new();
    private readonly Subject<CharacterUpdateMessage> _characterUpdates = new();
    private bool _disposed;

    /// <summary>
    /// Observable stream of time event messages.
    /// </summary>
    public IObservable<TimeEventMessage> TimeEvents => _timeEvents.AsObservable();

    /// <summary>
    /// Observable stream of time skip messages.
    /// </summary>
    public IObservable<TimeSkipMessage> TimeSkips => _timeSkips.AsObservable();

    /// <summary>
    /// Observable stream of combat state messages.
    /// </summary>
    public IObservable<CombatStateMessage> CombatStates => _combatStates.AsObservable();

    /// <summary>
    /// Observable stream of time result messages.
    /// </summary>
    public IObservable<TimeResultMessage> TimeResults => _timeResults.AsObservable();

    /// <summary>
    /// Observable stream of activity log messages.
    /// </summary>
    public IObservable<ActivityLogMessage> ActivityLogs => _activityLogs.AsObservable();

    /// <summary>
    /// Observable stream of character update messages.
    /// </summary>
    public IObservable<CharacterUpdateMessage> CharacterUpdates => _characterUpdates.AsObservable();

    /// <summary>
    /// Publishes a time event message to all subscribers.
    /// </summary>
    public void PublishTimeEvent(TimeEventMessage message)
    {
        _timeEvents.OnNext(message);
    }

    /// <summary>
    /// Publishes a time skip message to all subscribers.
    /// </summary>
    public void PublishTimeSkip(TimeSkipMessage message)
    {
        _timeSkips.OnNext(message);
    }

    /// <summary>
    /// Publishes a combat state message to all subscribers.
    /// </summary>
    public void PublishCombatState(CombatStateMessage message)
    {
        _combatStates.OnNext(message);
    }

    /// <summary>
    /// Publishes a time result message to all subscribers.
    /// </summary>
    public void PublishTimeResult(TimeResultMessage message)
    {
        _timeResults.OnNext(message);
    }

    /// <summary>
    /// Publishes an activity log message to all subscribers.
    /// </summary>
    public void PublishActivityLog(ActivityLogMessage message)
    {
        _activityLogs.OnNext(message);
    }

    /// <summary>
    /// Publishes a character update message to all subscribers.
    /// </summary>
    public void PublishCharacterUpdate(CharacterUpdateMessage message)
    {
        _characterUpdates.OnNext(message);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _timeEvents.Dispose();
        _timeSkips.Dispose();
        _combatStates.Dispose();
        _timeResults.Dispose();
        _activityLogs.Dispose();
        _characterUpdates.Dispose();
    }
}
