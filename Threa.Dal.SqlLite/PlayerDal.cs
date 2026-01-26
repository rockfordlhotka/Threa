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

                CREATE UNIQUE INDEX IF NOT EXISTS 
                IX_Players_Email ON Players (Email);

                CREATE UNIQUE INDEX IF NOT EXISTS 
                IX_Players_Id ON Players (Id);"
            ;
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error creating player table", ex);
        }
    }

    public async Task<IEnumerable<Player>> GetAllPlayersAsync()
    {
        try
        {
            var sql = "SELECT Id, Json FROM Players";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            using var reader = await command.ExecuteReaderAsync();
            var players = new List<Player>();
            while (reader.Read())
            {
                var id = reader.GetInt32(0);
                var json = reader.GetString(1);
                var player = System.Text.Json.JsonSerializer.Deserialize<Player>(json);
                if (player != null)
                {
                    player.Id = id;
                    players.Add(player);
                }
            }
            return players;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting all players", ex);
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

    public async Task<Player?> GetPlayerAsync(int id)
    {
        try
        {
            var sql = "SELECT Json FROM Players WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", id);
            using var reader = await command.ExecuteReaderAsync();
            if (!reader.Read())
                return null;
            string json = reader.GetString(0);
            var result = System.Text.Json.JsonSerializer.Deserialize<Player>(json);
            return result ?? throw new OperationFailedException($"Player {id} not found");
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error getting player {id}", ex);
        }
    }

    public async Task<Player?> GetPlayerByEmailAsync(string email)
    {
        try
        {
            var sql = "SELECT Id, Json FROM Players WHERE Email = @Email";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Email", email);
            using var reader = await command.ExecuteReaderAsync();
            if (!reader.Read())
                return null;
            var id = reader.GetInt32(0);
            var json = reader.GetString(1);
            var result = System.Text.Json.JsonSerializer.Deserialize<Player>(json);
            if (result != null)
                result.Id = id;
            return result;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error getting player {email}", ex);
        }
    }

    public async Task<Player> GetPlayerByEmailAsync(string email, string password)
    {
        try
        {
            var sql = "SELECT Id, Json FROM Players WHERE Email = @Email";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Email", email);
            using var reader = await command.ExecuteReaderAsync();
            if (!reader.Read())
                throw new NotFoundException($"{nameof(Player)} {email}");
            var id = reader.GetInt32(0);
            var json = reader.GetString(1);
            var result = System.Text.Json.JsonSerializer.Deserialize<Player>(json);
            if (result != null)
            {
                result.Id = id;
                if (!string.IsNullOrEmpty(password))
                {
                    // Verify the password using BCrypt
                    if (string.IsNullOrEmpty(result.HashedPassword) || 
                        !BCrypt.Net.BCrypt.Verify(password, result.HashedPassword))
                        throw new NotFoundException($"{nameof(Player)} {email}");
                }
                return result;
            }
            else
            {
                throw new NotFoundException($"{nameof(Player)} {email}");
            }
        }
        catch (NotFoundException)
        {
            throw;
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
            using var command = Connection.CreateCommand();
            if (player.Id < 0)
            {
                sql = "INSERT INTO Players (Email, Json) VALUES (@Email, @Json)";
                command.Parameters.AddWithValue("@Email", player.Email);
            }
            else
            {
                sql = "UPDATE Players SET Json = @Json WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", player.Id);
            }
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Json", System.Text.Json.JsonSerializer.Serialize(player));
            await command.ExecuteNonQueryAsync();

            if (player.Id < 0)
            {
                sql = "SELECT last_insert_rowid()";
                using var idCommand = Connection.CreateCommand();
                idCommand.CommandText = sql;
                long? lastInsertId = (long?)idCommand.ExecuteScalar();
                if (lastInsertId.HasValue)
                {
                    player.Id = (int)lastInsertId.Value;
                }
            }
            return player;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error saving player: {ex.Message}", ex);
        }
    }

    public async Task ChangePassword(int id, string oldPassword, string newPassword)
    {
        var existingPlayer = await GetPlayerAsync(id)
            ?? throw new NotFoundException($"{nameof(Player)} {id}");
        try
        {
            if (string.IsNullOrWhiteSpace(existingPlayer.Salt))
                existingPlayer.Salt = BCrypt.Net.BCrypt.GenerateSalt(12);

            if (!string.IsNullOrWhiteSpace(existingPlayer.HashedPassword))
            {
                var oldHashedPassword = BCrypt.Net.BCrypt.HashPassword(oldPassword, existingPlayer.Salt);
                if (existingPlayer.HashedPassword != oldHashedPassword)
                    throw new OperationFailedException($"Old password doesn't match for {nameof(Player)} {id}");
            }

            existingPlayer.HashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword, existingPlayer.Salt);

            var sql = "UPDATE Players SET Json = @Json WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", existingPlayer.Id);
            command.Parameters.AddWithValue("@Json", System.Text.Json.JsonSerializer.Serialize(existingPlayer));
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error changing password", ex);
        }
    }

    public async Task<string?> GetSecretQuestionAsync(string username)
    {
        var player = await GetPlayerByEmailAsync(username);
        // Return null for unknown user (prevents enumeration)
        return player?.SecretQuestion;
    }

    public async Task<bool> IsRecoveryLockedOutAsync(string username)
    {
        var player = await GetPlayerByEmailAsync(username);
        if (player == null) return false;

        return player.RecoveryLockoutUntil.HasValue &&
               player.RecoveryLockoutUntil.Value > DateTime.UtcNow;
    }

    public async Task<int> GetRemainingRecoveryAttemptsAsync(string username)
    {
        var player = await GetPlayerByEmailAsync(username);
        if (player == null) return 3; // Default for unknown (don't reveal)

        const int maxAttempts = 3;
        var remaining = maxAttempts - player.FailedRecoveryAttempts;
        return remaining > 0 ? remaining : 0;
    }

    public async Task<bool> ValidateSecretAnswerAsync(string username, string answer)
    {
        var player = await GetPlayerByEmailAsync(username);
        if (player == null) return false;

        // Check lockout
        if (player.RecoveryLockoutUntil.HasValue &&
            player.RecoveryLockoutUntil.Value > DateTime.UtcNow)
        {
            return false;
        }

        // Clear expired lockout
        if (player.RecoveryLockoutUntil.HasValue &&
            player.RecoveryLockoutUntil.Value <= DateTime.UtcNow)
        {
            player.FailedRecoveryAttempts = 0;
            player.RecoveryLockoutUntil = null;
        }

        // Normalize answer (trim + lowercase) and compare
        var normalizedAnswer = answer?.Trim().ToLowerInvariant() ?? string.Empty;
        var isCorrect = player.SecretAnswer == normalizedAnswer;

        if (isCorrect)
        {
            // Reset attempts on success
            player.FailedRecoveryAttempts = 0;
            player.RecoveryLockoutUntil = null;
        }
        else
        {
            // Increment failed attempts
            player.FailedRecoveryAttempts++;

            // Lockout after 3 failed attempts (15 minutes)
            if (player.FailedRecoveryAttempts >= 3)
            {
                player.RecoveryLockoutUntil = DateTime.UtcNow.AddMinutes(15);
            }
        }

        // Persist changes
        await SavePlayerAsync(player);

        return isCorrect;
    }

    public async Task ResetPasswordAsync(string username, string newPassword)
    {
        var player = await GetPlayerByEmailAsync(username)
            ?? throw new NotFoundException($"Player {username}");

        // Hash new password with BCrypt
        var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
        player.Salt = salt;
        player.HashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword, salt);

        // Clear recovery state
        player.FailedRecoveryAttempts = 0;
        player.RecoveryLockoutUntil = null;

        await SavePlayerAsync(player);
    }
}
