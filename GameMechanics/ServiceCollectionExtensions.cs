using GameMechanics.Combat.Effects;
using GameMechanics.Effects;
using GameMechanics.Items;
using Microsoft.Extensions.DependencyInjection;

namespace GameMechanics;

/// <summary>
/// Extension methods for configuring GameMechanics services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds GameMechanics services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddGameMechanics(this IServiceCollection services)
    {
        // Register effect services
        services.AddScoped<ItemEffectService>();

        // Register attack effect service for on-hit effect triggering
        services.AddScoped<AttackEffectService>();

        // Register item management service
        services.AddScoped<ItemManagementService>();

        return services;
    }
}
