using Microsoft.Extensions.DependencyInjection;

namespace Threa.Dal.SqlServer
{
  public static  class ConfigurationExtensions
  {
    public static void AddSqlDb(this IServiceCollection services)
    {
      services.AddTransient<IPlayerDal, PlayerDal>();
      services.AddTransient<ICharacterDal, CharacterDal>();
    }
  }
}
