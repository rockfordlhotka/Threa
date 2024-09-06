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
      lock (MockDb.Players)
      {
        var player = MockDb.Players.Where(r => r.Id == id).FirstOrDefault();
        if (player == null)
          throw new NotFoundException(nameof(player));
        MockDb.Players.Remove(player);
        return Task.CompletedTask;
      }
    }

    public Task<Player> GetPlayerAsync(int id)
    {
      lock (MockDb.Players)
      {
        var player = MockDb.Players.Where(r => r.Id == id).FirstOrDefault();
        return Task.FromResult(player);
      }
    }

    public Task<Player> GetPlayerByEmailAsync(string email)
    {
      lock (MockDb.Players)
      {
        try
        {
          var players = MockDb.Players;
          var player = players.Where(r => r.Email == email).FirstOrDefault();
          return Task.FromResult(player);
        }
        catch (Exception ex)
        {
          var x = ex;
        }
        return Task.FromResult(((Player)new Player()));
      }
    }

    public Task<Player> SavePlayerAsync(Player obj)
    {
      lock (MockDb.Players)
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
}
