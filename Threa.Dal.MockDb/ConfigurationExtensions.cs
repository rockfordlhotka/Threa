using Microsoft.Extensions.DependencyInjection;
using Threa.Dal.MockDb;

namespace Threa.Dal;

public static class ConfigurationExtensions
{
    public static void AddMockDb(this IServiceCollection services)
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
    }
}
