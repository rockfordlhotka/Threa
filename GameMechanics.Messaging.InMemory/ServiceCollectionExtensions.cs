using GameMechanics.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace GameMechanics.Messaging.InMemory;

/// <summary>
/// Extension methods for configuring in-memory messaging services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds in-memory time event messaging to the service collection.
    /// This is suitable for single-server Blazor Server deployments where all
    /// users are connected to the same process.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInMemoryMessaging(this IServiceCollection services)
    {
        // Register the shared message bus as singleton
        services.AddSingleton<InMemoryMessageBus>();

        // Register publisher and subscriber (they share the same bus)
        services.AddSingleton<ITimeEventPublisher, InMemoryTimeEventPublisher>();
        services.AddSingleton<ITimeEventSubscriber, InMemoryTimeEventSubscriber>();

        // Register the activity log service
        services.AddSingleton<IActivityLogService, InMemoryActivityLogService>();

        return services;
    }
}
