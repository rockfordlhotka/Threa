using GameMechanics.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace GameMechanics.Messaging.RabbitMQ;

/// <summary>
/// Extension methods for configuring RabbitMQ messaging services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds RabbitMQ-based time event messaging to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional configuration for messaging options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRabbitMqMessaging(
        this IServiceCollection services,
        Action<MessagingOptions>? configureOptions = null)
    {
        // Configure options
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<MessagingOptions>(_ => { });
        }

        // Register publisher and subscriber as singletons (they maintain connections)
        services.AddSingleton<ITimeEventPublisher, RabbitMqTimeEventPublisher>();
        services.AddSingleton<ITimeEventSubscriber, RabbitMqTimeEventSubscriber>();

        return services;
    }

    /// <summary>
    /// Adds RabbitMQ-based time event messaging using configuration section.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configurationSection">The configuration section containing MessagingOptions.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRabbitMqMessaging(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfigurationSection configurationSection)
    {
        services.Configure<MessagingOptions>(configurationSection);
        
        services.AddSingleton<ITimeEventPublisher, RabbitMqTimeEventPublisher>();
        services.AddSingleton<ITimeEventSubscriber, RabbitMqTimeEventSubscriber>();

        return services;
    }

    /// <summary>
    /// Adds the time event handler hosted service which bridges messaging with TimeManager.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTimeEventHandlerService(this IServiceCollection services)
    {
        services.AddSingleton<TimeEventHandlerService>();
        services.AddHostedService(sp => sp.GetRequiredService<TimeEventHandlerService>());

        return services;
    }
}
