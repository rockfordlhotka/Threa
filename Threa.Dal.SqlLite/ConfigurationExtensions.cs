using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Threa.Dal.Sqlite;

namespace Threa.Dal;

public static class SqliteConfigurationExtensions
{
    /// <summary>
    /// Registers all SQLite DAL implementations for production use.
    /// </summary>
    public static void AddSqlite(this IServiceCollection services)
    {
        RegisterDalServices(services);
        services.AddScoped<SqliteConnection>(provider =>
        {
            var config = provider.GetService<IConfiguration>();
            var dbPath = config?["ConnectionStrings:Sqlite"] ?? "threa.db";
            return CreateConnection(dbPath);
        });
    }

    /// <summary>
    /// Registers all SQLite DAL implementations for testing with a specific database path.
    /// Each test class should use a unique database file for isolation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="databasePath">Path to the test database file (e.g., test_{className}_{guid}.db).</param>
    public static void AddTestSqlite(this IServiceCollection services, string databasePath)
    {
        RegisterDalServices(services);
        services.AddScoped<SqliteConnection>(provider => CreateConnection(databasePath));
    }

    private static void RegisterDalServices(IServiceCollection services)
    {
        services.AddTransient<IPlayerDal, PlayerDal>();
        services.AddTransient<ICharacterDal, CharacterDal>();
        services.AddTransient<IImageDal, ImageDal>();
        services.AddTransient<ISpeciesDal, SpeciesDal>();
        services.AddTransient<IItemTemplateDal, ItemTemplateDal>();
        services.AddTransient<ICharacterItemDal, CharacterItemDal>();
        services.AddTransient<IEffectDefinitionDal, EffectDefinitionDal>();
        services.AddTransient<ICharacterEffectDal, CharacterEffectDal>();
        services.AddTransient<IItemEffectDal, ItemEffectDal>();
        services.AddTransient<ISkillDal, SkillDal>();
        services.AddTransient<IMagicSchoolDal, MagicSchoolDal>();
        services.AddTransient<ITableDal, TableDal>();
        services.AddTransient<IEffectTemplateDal, EffectTemplateDal>();
        services.AddTransient<IJoinRequestDal, JoinRequestDal>();
        services.AddTransient<IManaDal, ManaDal>();
        services.AddTransient<ISpellDefinitionDal, SpellDefinitionDal>();
        services.AddTransient<ILocationEffectDal, LocationEffectDal>();
    }

    private static SqliteConnection CreateConnection(string dbPath)
    {
        // Use shared cache and disable connection caching to ensure cross-connection visibility
        var conn = new SqliteConnection($"Data Source={dbPath};Cache=Shared");
        conn.Open();
        // Enable WAL mode for better concurrent read/write support
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL; PRAGMA read_uncommitted=1;";
            cmd.ExecuteNonQuery();
        }
        return conn;
    }
}
