using Microsoft.Extensions.DependencyInjection;

namespace Threa.Dal.MockDb
{
  public static  class ConfigurationExtensions
  {
    public static void AddMockDb(this IServiceCollection services)
    {
      services.AddTransient<IPlayerDal, PlayerDal>();
      services.AddTransient<ICharacterDal, CharacterDal>();
    }
  }
}
