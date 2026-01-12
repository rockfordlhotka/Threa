using System;

namespace GameMechanics.Messaging;

/// <summary>
/// Service for publishing and subscribing to activity log messages across GM and player screens.
/// </summary>
public interface IActivityLogService
{
    /// <summary>
    /// Publishes an activity log message to all subscribers at the specified table.
    /// </summary>
    /// <param name="tableId">The table to publish to.</param>
    /// <param name="message">The activity message text.</param>
    /// <param name="source">The source of the activity (GM, character name, or system).</param>
    /// <param name="category">The category of activity.</param>
    void Publish(Guid tableId, string message, string source, ActivityCategory category = ActivityCategory.General);

    /// <summary>
    /// Publishes an activity log message to all subscribers at the specified table.
    /// </summary>
    /// <param name="message">The complete activity log message.</param>
    void Publish(ActivityLogMessage message);

    /// <summary>
    /// Subscribes to activity log messages for a specific table.
    /// </summary>
    /// <param name="tableId">The table to subscribe to.</param>
    /// <param name="onMessage">Callback invoked when a message is received.</param>
    /// <returns>Disposable subscription handle.</returns>
    IDisposable Subscribe(Guid tableId, Action<ActivityLogMessage> onMessage);

    /// <summary>
    /// Gets an observable stream of activity log messages for a specific table.
    /// </summary>
    /// <param name="tableId">The table to get messages for.</param>
    /// <returns>Observable stream of messages.</returns>
    IObservable<ActivityLogMessage> GetStream(Guid tableId);
}
