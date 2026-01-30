using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Threa.Dal.Dto;

namespace Threa.Dal.Sqlite;

/// <summary>
/// SQLite implementation of join request data access.
/// </summary>
public class JoinRequestDal : IJoinRequestDal
{
    private readonly SqliteConnection Connection;

    public JoinRequestDal(SqliteConnection connection)
    {
        Connection = connection;
        InitializeTable();
    }

    private void InitializeTable()
    {
        try
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS JoinRequests (
                    Id TEXT NOT NULL PRIMARY KEY,
                    CharacterId INTEGER NOT NULL,
                    TableId TEXT NOT NULL,
                    PlayerId INTEGER NOT NULL,
                    Status INTEGER NOT NULL,
                    Json TEXT
                );
                CREATE INDEX IF NOT EXISTS IX_JoinRequests_PlayerId ON JoinRequests(PlayerId);
                CREATE INDEX IF NOT EXISTS IX_JoinRequests_TableId ON JoinRequests(TableId);
                CREATE INDEX IF NOT EXISTS IX_JoinRequests_CharacterId ON JoinRequests(CharacterId);
            ";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error creating JoinRequests table", ex);
        }
    }

    public JoinRequest GetBlank()
    {
        return new JoinRequest
        {
            Id = Guid.NewGuid(),
            RequestedAt = DateTime.UtcNow,
            Status = JoinRequestStatus.Pending
        };
    }

    public async Task<List<JoinRequest>> GetRequestsByPlayerAsync(int playerId)
    {
        try
        {
            var sql = "SELECT Json FROM JoinRequests WHERE PlayerId = @PlayerId AND Status != @DeniedStatus";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@PlayerId", playerId);
            command.Parameters.AddWithValue("@DeniedStatus", (int)JoinRequestStatus.Denied);
            using var reader = await command.ExecuteReaderAsync();
            var results = new List<JoinRequest>();
            while (reader.Read())
            {
                string json = reader.GetString(0);
                var request = JsonSerializer.Deserialize<JoinRequest>(json);
                if (request != null)
                    results.Add(request);
            }
            return results;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting join requests by player", ex);
        }
    }

    public async Task<List<JoinRequest>> GetPendingRequestsForTableAsync(Guid tableId)
    {
        try
        {
            var sql = "SELECT Json FROM JoinRequests WHERE TableId = @TableId AND Status = @PendingStatus";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@TableId", tableId.ToString());
            command.Parameters.AddWithValue("@PendingStatus", (int)JoinRequestStatus.Pending);
            using var reader = await command.ExecuteReaderAsync();
            var results = new List<JoinRequest>();
            while (reader.Read())
            {
                string json = reader.GetString(0);
                var request = JsonSerializer.Deserialize<JoinRequest>(json);
                if (request != null)
                    results.Add(request);
            }
            return results;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting pending join requests for table", ex);
        }
    }

    public async Task<JoinRequest?> GetPendingRequestAsync(int characterId, Guid tableId)
    {
        try
        {
            var sql = "SELECT Json FROM JoinRequests WHERE CharacterId = @CharacterId AND TableId = @TableId AND Status = @PendingStatus";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@CharacterId", characterId);
            command.Parameters.AddWithValue("@TableId", tableId.ToString());
            command.Parameters.AddWithValue("@PendingStatus", (int)JoinRequestStatus.Pending);
            using var reader = await command.ExecuteReaderAsync();
            if (!reader.Read())
                return null;
            string json = reader.GetString(0);
            return JsonSerializer.Deserialize<JoinRequest>(json);
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting pending join request", ex);
        }
    }

    public async Task<JoinRequest?> GetRequestAsync(Guid id)
    {
        try
        {
            var sql = "SELECT Json FROM JoinRequests WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", id.ToString());
            using var reader = await command.ExecuteReaderAsync();
            if (!reader.Read())
                return null;
            string json = reader.GetString(0);
            return JsonSerializer.Deserialize<JoinRequest>(json);
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting join request", ex);
        }
    }

    public async Task<JoinRequest> SaveRequestAsync(JoinRequest request)
    {
        try
        {
            var checkSql = "SELECT COUNT(*) FROM JoinRequests WHERE Id = @Id";
            using var checkCommand = Connection.CreateCommand();
            checkCommand.CommandText = checkSql;
            checkCommand.Parameters.AddWithValue("@Id", request.Id.ToString());
            var exists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;

            string sql;
            if (exists)
            {
                sql = @"UPDATE JoinRequests 
                        SET CharacterId = @CharacterId, TableId = @TableId, PlayerId = @PlayerId, Status = @Status, Json = @Json 
                        WHERE Id = @Id";
            }
            else
            {
                sql = @"INSERT INTO JoinRequests (Id, CharacterId, TableId, PlayerId, Status, Json) 
                        VALUES (@Id, @CharacterId, @TableId, @PlayerId, @Status, @Json)";
            }

            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", request.Id.ToString());
            command.Parameters.AddWithValue("@CharacterId", request.CharacterId);
            command.Parameters.AddWithValue("@TableId", request.TableId.ToString());
            command.Parameters.AddWithValue("@PlayerId", request.PlayerId);
            command.Parameters.AddWithValue("@Status", (int)request.Status);
            command.Parameters.AddWithValue("@Json", JsonSerializer.Serialize(request));
            await command.ExecuteNonQueryAsync();

            return request;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error saving join request", ex);
        }
    }

    public async Task DeleteRequestAsync(Guid id)
    {
        try
        {
            var sql = "DELETE FROM JoinRequests WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", id.ToString());
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error deleting join request", ex);
        }
    }

    public async Task<int> GetPendingCountForTableAsync(Guid tableId)
    {
        try
        {
            var sql = "SELECT COUNT(*) FROM JoinRequests WHERE TableId = @TableId AND Status = @PendingStatus";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@TableId", tableId.ToString());
            command.Parameters.AddWithValue("@PendingStatus", (int)JoinRequestStatus.Pending);
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting pending count for table", ex);
        }
    }

    public async Task DeleteRequestsByCharacterAsync(int characterId)
    {
        try
        {
            var sql = "DELETE FROM JoinRequests WHERE CharacterId = @CharacterId";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@CharacterId", characterId);
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error deleting join requests by character", ex);
        }
    }
}
