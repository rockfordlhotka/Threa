using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Threa.Dal.Sqlite;

namespace Threa.Dal;

public static class SqliteConfigurationExtensions
{
    public static void AddSqlite(this IServiceCollection services)
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
        services.AddScoped<SqliteConnection>(provider =>
        {
            var config = provider.GetService<IConfiguration>();
            var dbPath = config?["ConnectionStrings:Sqlite"] ?? "threa.db";
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
        });
    }
}
