using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal.MockDb
{
  public class PlayerDal : IPlayerDal
  {
    public Task DeletePlayerAsync(int id)
    {
      var player = MockDb.Players.Where(r => r.Id == id).FirstOrDefault();
      if (player == null)
        throw new NotFoundException(nameof(player));
      MockDb.Players.Remove(player);
      return Task.CompletedTask;
    }

    public Task<IPlayer> GetPlayerAsync(int id)
    {
      var player = MockDb.Players.Where(r => r.Id == id).FirstOrDefault();
      return Task.FromResult(player);
    }

    public Task<IPlayer> GetPlayerByEmailAsync(string email)
    {
      var player = MockDb.Players.Where(r => r.Email == email).FirstOrDefault();
      return Task.FromResult(player);
    }

    public Task<IPlayer> SavePlayerAsync(IPlayer obj)
    {
      var player = MockDb.Players.Where(r => r.Id == obj.Id).FirstOrDefault();
      if (player == null)
      {
        player = new Player
        {
          Id = MockDb.Players.Max(r => r.Id),
          Name = obj.Name,
          Email = obj.Email
        };
        MockDb.Players.Add(player);
      }
      else
      {
        player.Name = obj.Name;
        player.Email = obj.Email;
      }
      return Task.FromResult(player);
    }
  }
}
