using Csla.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Threa.Dal;
using Threa.Dal.Sqlite;

namespace GameMechanics.Test;

/// <summary>
/// Base class for all tests that need a SQLite database.
/// Provides isolated test databases for each test class.
/// </summary>
public abstract class TestBase
{
    /// <summary>
    /// The service provider for the current test.
    /// </summary>
    protected ServiceProvider? Services { get; private set; }

    /// <summary>
    /// The path to the test database file.
    /// </summary>
    protected string? DbPath { get; private set; }

    /// <summary>
    /// Initializes the service provider with a fresh test database.
    /// </summary>
    /// <param name="seedData">Whether to seed test data into the database.</param>
    /// <returns>The configured service provider.</returns>
    protected ServiceProvider InitServices(bool seedData = true)
    {
        // Create unique test database file
        DbPath = Path.Combine(Path.GetTempPath(), $"test_{GetType().Name}_{Guid.NewGuid():N}.db");

        var services = new ServiceCollection();
        services.AddCsla();
        services.AddTestSqlite(DbPath);
        ConfigureAdditionalServices(services);
        Services = services.BuildServiceProvider();

        if (seedData)
        {
            var seeder = new TestDataSeeder();
            seeder.SeedAllAsync(Services).GetAwaiter().GetResult();
        }

        return Services;
    }

    /// <summary>
    /// Override this method to register additional services.
    /// Called by InitServices before building the service provider.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    protected virtual void ConfigureAdditionalServices(IServiceCollection services)
    {
        // Default: no additional services
    }

    /// <summary>
    /// Cleans up test resources after each test.
    /// </summary>
    [TestCleanup]
    public virtual void Cleanup()
    {
        Services?.Dispose();
        Services = null;

        if (DbPath != null)
        {
            TryDelete(DbPath);
            TryDelete(DbPath + "-shm");
            TryDelete(DbPath + "-wal");
            DbPath = null;
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    /// <summary>
    /// Helper method to clear all players from the database.
    /// Use this in tests that need to verify first-user logic.
    /// </summary>
    protected async System.Threading.Tasks.Task ClearPlayersAsync()
    {
        if (Services == null)
            throw new InvalidOperationException("Services not initialized. Call InitServices() first.");

        var dal = Services.GetRequiredService<IPlayerDal>();
        var players = await dal.GetAllPlayersAsync();
        foreach (var player in players)
        {
            await dal.DeletePlayerAsync(player.Id);
        }
    }

    /// <summary>
    /// Helper method to add a player directly to the database.
    /// Use this in tests that need to set up specific player state.
    /// </summary>
    protected async System.Threading.Tasks.Task<Threa.Dal.Dto.Player> AddPlayerAsync(Threa.Dal.Dto.Player player)
    {
        if (Services == null)
            throw new InvalidOperationException("Services not initialized. Call InitServices() first.");

        var dal = Services.GetRequiredService<IPlayerDal>();
        return await dal.SavePlayerAsync(player);
    }

    /// <summary>
    /// Helper method to get a player by email.
    /// </summary>
    protected async System.Threading.Tasks.Task<Threa.Dal.Dto.Player?> GetPlayerByEmailAsync(string email)
    {
        if (Services == null)
            throw new InvalidOperationException("Services not initialized. Call InitServices() first.");

        var dal = Services.GetRequiredService<IPlayerDal>();
        return await dal.GetPlayerByEmailAsync(email);
    }
}
