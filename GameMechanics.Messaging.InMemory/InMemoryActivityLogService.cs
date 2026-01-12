using System.Reactive.Linq;
using Microsoft.Extensions.Logging;

namespace GameMechanics.Messaging.InMemory;

/// <summary>
/// In-memory implementation of <see cref="IActivityLogService"/> using Rx.NET.
/// </summary>
public class InMemoryActivityLogService : IActivityLogService
{
    private readonly InMemoryMessageBus _bus;
    private readonly ILogger<InMemoryActivityLogService> _logger;

    public InMemoryActivityLogService(
        InMemoryMessageBus bus,
        ILogger<InMemoryActivityLogService> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public void Publish(Guid tableId, string message, string source, ActivityCategory category = ActivityCategory.General)
    {
        Publish(new ActivityLogMessage
        {
            TableId = tableId,
            Message = message,
            Source = source,
            Category = category
        });
    }

    public void Publish(ActivityLogMessage message)
    {
        _logger.LogDebug("Publishing activity log: [{Category}] {Source}: {Message}",
            message.Category, message.Source, message.Message);

        _bus.PublishActivityLog(message);
    }

    public IDisposable Subscribe(Guid tableId, Action<ActivityLogMessage> onMessage)
    {
        _logger.LogDebug("Subscribing to activity log for table {TableId}", tableId);

        return GetStream(tableId).Subscribe(onMessage);
    }

    public IObservable<ActivityLogMessage> GetStream(Guid tableId)
    {
        return _bus.ActivityLogs.Where(msg => msg.TableId == tableId);
    }
}
