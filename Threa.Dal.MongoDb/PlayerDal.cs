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

    public async Task DeletePlayerAsync(string id)
    {
      var data = db.GetCollection<Player>();
      var result = await data.DeleteOneAsync(i => i.Id == id);
      if (!result.IsAcknowledged)
        throw new NotFoundException($"{nameof(Player)}: {id}");
    }

    public async Task<IEnumerable<ICharacter>> GetCharactersAsync(string playerId)
    {
      var data = db.GetCollection<Character>();
      var result = await data.Find(i => i.PlayerId == playerId).ToListAsync();
      return result;
    }

    public async Task<IPlayer> GetPlayerAsync(string id)
    {
      var data = db.GetCollection<Player>();
      var result = await data.Find(i => i.Id == id).FirstOrDefaultAsync();
      return result;
    }

    public async Task<IPlayer> GetPlayerByEmailAsync(string email)
    {
      var data = db.GetCollection<Player>();
      var result = await data.Find(i => i.Email == email).FirstOrDefaultAsync();
      return result;
    }

    public async Task<IPlayer> SavePlayerAsync(IPlayer obj)
    {
      if (!(obj is Player player))
        throw new ArgumentException(nameof(obj));

      var data = db.GetCollection<Player>();
      var oldDocument = await GetPlayerAsync(obj.Id);
      if (oldDocument == null)
      {
        try
        {
          await data.InsertOneAsync(player);
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
          await data.ReplaceOneAsync(i => i.Id == player.Id, player);
        }
        catch (Exception ex)
        {
          throw new OperationFailedException($"Update {nameof(Player)}: {obj?.Id}", ex);
        }
      }
      return player;
    }
  }
}
