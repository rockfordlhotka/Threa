using Csla;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;
using Threa.Dal;
using Threa.Dal.MockDb;

namespace GameMechanics.Test
{
  [TestClass]
  public class PlayerTest
  {
    [TestMethod]
    [Ignore]
    public async Task InsertPlayer()
    {
      var player = new Threa.Dal.MongoDb.Player();
      player.Email = "test@example.com";
      player.Name = "Test McTesty";
      var config = new Threa.Dal.MongoDb.ThreaDbSettings();
      var context = new Threa.Dal.MongoDb.ThreaDbContext(config);
      var dal = new Threa.Dal.MongoDb.PlayerDal(context);
      var result = await dal.SavePlayerAsync(player);
      Assert.AreEqual(player.Name, result.Name);
    }
  }
}
