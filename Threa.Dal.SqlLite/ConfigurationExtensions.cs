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
        services.AddScoped<SqliteConnection>(provider =>
        {
            var config = provider.GetService<IConfiguration>();
            var dbPath = config?["ConnectionStrings:Sqlite"] ?? "threa.db";
            var conn = new SqliteConnection($"Data Source={dbPath}");
            conn.Open();
            return conn;
        });
    }
}
