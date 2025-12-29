using Microsoft.Data.Sqlite;
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
        services.AddTransient<IItemTemplateDal, ItemTemplateDal>();
        services.AddTransient<ICharacterItemDal, CharacterItemDal>();
        services.AddTransient<IEffectDefinitionDal, EffectDefinitionDal>();
        services.AddTransient<ICharacterEffectDal, CharacterEffectDal>();
        services.AddTransient<IItemEffectDal, ItemEffectDal>();
        services.AddScoped<SqliteConnection>(provider => 
        {
            var conn = new SqliteConnection("Data Source=threa.db");
            conn.Open();
            return conn;
        });
    }
}
