using GameMechanics.Actions;
using GameMechanics.Batch;
using GameMechanics.Combat.Effects;
using GameMechanics.Effects;
using GameMechanics.Items;
using GameMechanics.Time;
using GameMechanics.Time.Calendars;
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
        // Register time formatting service (singleton - stateless)
        services.AddSingleton<IGameTimeFormatService, DefaultGameTimeFormatService>();

        // Register calendar systems
        services.AddSingleton<IGameCalendar, CommonCalendar>();
        services.AddSingleton<IGameCalendar, TriumvirateCalendar>();
        services.AddSingleton<IGameCalendar, StandardFantasyCalendar>();
        services.AddSingleton<IGameCalendar, ConfederateCalendar>();
        services.AddSingleton<IGameCalendar, ImperialCalendar>();
        services.AddSingleton<IGameCalendarProvider, GameCalendarProvider>();

        // Register travel calculator service
        services.AddScoped<ITravelCalculator, TravelCalculator>();

        // Register effect services
        services.AddScoped<ItemEffectService>();

        // Register attack effect service for on-hit effect triggering
        services.AddScoped<AttackEffectService>();

        // Register item management service
        services.AddScoped<ItemManagementService>();

        // Register time advancement service for server-side time processing
        services.AddScoped<TimeAdvancementService>();

        // Register batch action service for multi-character operations (damage/healing, visibility, dismiss)
        services.AddScoped<BatchActionService>();

        return services;
    }
}
