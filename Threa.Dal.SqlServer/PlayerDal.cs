using Microsoft.Data.SqlClient;
using System;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal.Dto;
using Dapper;

namespace Threa.Dal.SqlServer
{
  public class PlayerDal : IPlayerDal
  {
    private readonly SqlConnection db;

    public PlayerDal(SqlConnection db)
    {
      this.db = db;
    }

    public async Task DeletePlayerAsync(int id)
    {
      var sql = "DELETE FROM Player WHERE ID = @id";
      var affectedRows = await db.ExecuteAsync(sql, new { id });
      if (affectedRows == 0)
        throw new NotFoundException($"Player {id}");
    }

    public async Task<IPlayer> GetPlayerAsync(int id)
    {
      var sql = "SELECT * FROM Player WHERE ID = @id";
      var data = (await db.QueryAsync<Player>(sql, new { id })).FirstOrDefault();
      if (data != null)
        throw new NotFoundException($"Player {id}");
      return (IPlayer)data;
    }

    public async Task<IPlayer> GetPlayerByEmailAsync(string email)
    {
      var sql = "SELECT * FROM Player WHERE [Email] = @email";
      var data = (await db.QueryAsync<Player>(sql, new { email })).FirstOrDefault();
      return (IPlayer)data;
    }

    public async Task<IPlayer> SavePlayerAsync(IPlayer player)
    {
      var sql = "SELECT * FROM Player WHERE ID = @id";
      var data = (await db.QueryAsync<Player>(sql, new { id = player.Id })).FirstOrDefault();
      if (data == null)
      {
        sql = @"INSERT INTO Player (
            [Email],[Name],[LastUsed],[ImageUrl]) 
          VALUES (
            @Email,@Name,@LastUsed,@ImageUrl);
          SELECT CAST(SCOPE_IDENTITY() AS INT)";
        player.Id = (await db.QueryAsync<int>(sql, GetParams(player))).Single();
        if (player.Id == 0)
          throw new OperationFailedException($"Insert Player {player.Id}");
      }
      else
      {
        sql = @"UPDATE Player SET 
                [Email] = @Email,
                [Name] = @Name,
                [LastUsed] = @LastUsed,
                [ImageUrl] = @ImageUrl
              WHERE Id = @id";
        var affectedRows = await db.ExecuteAsync(sql, GetParams(player));
        if (affectedRows == 0)
          throw new OperationFailedException($"Update Player {player.Id}");
      }
      return player;
    }

    private object GetParams(IPlayer player)
    {
      return new
      {
        player.Email,
        player.Name,
        LastUsed = DateTime.Now,
        player.ImageUrl,
        player.Id
      };
    }
  }
}
