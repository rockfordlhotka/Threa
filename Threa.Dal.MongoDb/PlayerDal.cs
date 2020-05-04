using System;
using System.Collections.Generic;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace Threa.Dal.MongoDb
{
  public class PlayerDal : IPlayerDal
  {
    private readonly ThreaDbContext db;
    public PlayerDal(ThreaDbContext db)
    {
      this.db = db;
    }

    public async Task DeletePlayer(string id)
    {
      var data = db.GetCollection<Player>();
      var result = await data.DeleteOneAsync(i => i.Id == id);
      if (!result.IsAcknowledged)
        throw new NotFoundException($"{nameof(Player)}: {id}");
    }

    public async Task<IEnumerable<ICharacter>> GetCharacters(string playerId)
    {
      var data = db.GetCollection<Character>();
      var result = await data.Find(i => i.PlayerId == playerId).ToListAsync();
      if (result == null)
        throw new NotFoundException($"{nameof(Character)}: {playerId}");
      return result;
    }

    public async Task<IPlayer> GetPlayer(string id)
    {
      var data = db.GetCollection<Player>();
      var result = await data.Find(i => i.Id == id).FirstOrDefaultAsync();
      if (result == null)
        throw new NotFoundException($"{nameof(Player)}: {id}");
      return result;
    }

    public async Task<IPlayer> SavePlayer(IPlayer obj)
    {
      if (!(obj is Player player))
        throw new ArgumentException(nameof(obj));

      var data = db.GetCollection<Player>();
      var oldDocument = await data.Find(i => i.Id == player.Id).FirstOrDefaultAsync();
      if (oldDocument == null)
      {
        try
        {
          await data.InsertOneAsync(player);
          return player;
        }
        catch (Exception ex)
        {
          throw new OperationFailedException($"Insert {nameof(Player)}: {obj?.Id}", ex);
        }
      }
      else
      {
        try
        {
          var update = Builders<Player>.Update.Set(nameof(player.Id), player.Id);
          update = update.Set(nameof(player.Name), player.Name);
          update = update.Set(nameof(player.Email), player.Email);
          var result = data.UpdateOne(i => i.Id == player.Id, update);
          return player;
        }
        catch (Exception ex)
        {
          throw new OperationFailedException($"Update {nameof(Player)}: {obj?.Id}", ex);
        }
      }
    }
  }
}
