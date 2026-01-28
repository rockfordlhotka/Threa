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

        // Register publisher as singleton (shared across all circuits)
        services.AddSingleton<ITimeEventPublisher, InMemoryTimeEventPublisher>();

        // Register subscriber as scoped (one per SignalR circuit) to prevent cross-circuit interference
        // Each circuit needs its own subscriber instance to manage event handlers independently
        services.AddScoped<ITimeEventSubscriber, InMemoryTimeEventSubscriber>();

        // Register the activity log service
        services.AddSingleton<IActivityLogService, InMemoryActivityLogService>();

        return services;
    }
}
