using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Threa.Dal;

namespace GameMechanics.Test;

[TestClass]
public class PlayerTest : TestBase
{
    [TestMethod]
    public async Task DatabaseInitializationCheck()
    {
        var provider = InitServices();
        var playerDal = provider.GetRequiredService<IPlayerDal>();
        var characterDal = provider.GetRequiredService<ICharacterDal>();

        var players = await playerDal.GetAllPlayersAsync();
        var characters = await characterDal.GetAllCharactersAsync();

        Assert.IsNotNull(players);
        Assert.IsNotNull(characters);
    }
}
