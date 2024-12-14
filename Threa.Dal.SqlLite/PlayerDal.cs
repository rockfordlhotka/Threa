using Microsoft.Data.Sqlite;
using Threa.Dal.Dto;

namespace Threa.Dal.Sqlite;

public class PlayerDal : IPlayerDal
{
    private readonly SqliteConnection Connection;

    public PlayerDal(SqliteConnection connection)
    {
        Connection = connection;
        try
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS 
                Players (
                    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    Email TEXT NOT NULL,
                    Json TEXT
                );
            ";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error creating player table", ex);
        }
    }

    public async Task DeletePlayerAsync(int id)
    {
        try
        {
            var sql = "DELETE FROM Players WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", id);
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error deleting player {id}", ex);
        }
    }

    public async Task<Player> GetPlayerAsync(int id)
    {
        try
        {
            var sql = "SELECT Json FROM Players WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", id);
            using var reader = await command.ExecuteReaderAsync();
            if (!reader.Read())
                throw new NotFoundException($"{nameof(Player)} {id}");
            string json = reader.GetString(0);
            var result = System.Text.Json.JsonSerializer.Deserialize<Player>(json);
            return result ?? throw new OperationFailedException($"Player {id} not found");
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error getting player {id}", ex);
        }
    }

    public async Task<Player> GetPlayerByEmailAsync(string email, string hashedPassword)
    {
        try
        {
            var sql = "SELECT Json FROM Players WHERE Email = @Email";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Email", email);
            using var reader = await command.ExecuteReaderAsync();
            if (!reader.Read())
                throw new NotFoundException($"{nameof(Player)} {email}");
            string json = reader.GetString(0);
            var result = System.Text.Json.JsonSerializer.Deserialize<Player>(json);
            if (result != null)
            {
                if (hashedPassword != result.HashedPassword)
                    throw new NotFoundException($"{nameof(Player)} {email}");
                return result;
            }
            else
            {
                throw new NotFoundException($"{nameof(Player)} {email}");
            }
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error getting player {email}", ex);
        }
    }

    public async Task<Player> SavePlayerAsync(Player player)
    {
        try
        {
            string sql;
            if (player.Id == 0)
            {
                sql = "INSERT INTO Players (Id, Email, Json) VALUES (@Id, @Email, @Json)";
            }
            else
            {
                sql = "UPDATE Players SET Id = @Id, Email = @Email, Json = @Json WHERE Id = @Id";
            }
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", player.Id);
            command.Parameters.AddWithValue("@Email", player.Email);
            command.Parameters.AddWithValue("@Json", System.Text.Json.JsonSerializer.Serialize(player));
            await command.ExecuteNonQueryAsync();

            if (player.Id == 0)
            {
                sql = "SELECT last_insert_rowid()";
                using var idCommand = Connection.CreateCommand();
                {
                    long? lastInsertId = (long?)idCommand.ExecuteScalar();
                    if (lastInsertId.HasValue)
                    {
                        player.Id = (int)lastInsertId.Value;
                    }
                }
            }
            return player;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error saving player", ex);
        }
    }
}
