using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal.MockDb
{
    public class PlayerDal : IPlayerDal
    {
        public Task<IEnumerable<Player>> GetAllPlayersAsync()
        {
            lock (MockDb.Players)
            {
                return Task.FromResult<IEnumerable<Player>>(MockDb.Players.ToList());
            }
        }

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

        public Task<Player?> GetPlayerAsync(int id)
        {
            lock (MockDb.Players)
            {
                var player = MockDb.Players.Where(r => r.Id == id).FirstOrDefault();
                return Task.FromResult(player);
            }
        }

        public Task<Player?> GetPlayerByEmailAsync(string email)
        {
            lock (MockDb.Players)
            {
                var player = MockDb.Players.Where(r => r.Email == email).FirstOrDefault();
                return Task.FromResult(player);
            }
        }

        public Task<Player> GetPlayerByEmailAsync(string email, string password)
        {
            lock (MockDb.Players)
            {
                var existingPlayer = MockDb.Players.Where(r => r.Email == email).FirstOrDefault()
                    ?? throw new NotFoundException($"{nameof(Player)} {email}");
                if (!string.IsNullOrWhiteSpace(existingPlayer.HashedPassword))
                {
                    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, existingPlayer.Salt);
                    if (existingPlayer.HashedPassword != hashedPassword)
                        throw new NotFoundException($"{nameof(Player)} {email}");
                }
                return Task.FromResult(existingPlayer);
            }
        }

        public Task<Player> SavePlayerAsync(Player obj)
        {
            lock (MockDb.Players)
            {
                var existingPlayer = MockDb.Players.Where(r => r.Email == obj.Email).FirstOrDefault();
                if (existingPlayer == null)
                {
                    // Use provided salt if available, otherwise generate one
                    var salt = !string.IsNullOrWhiteSpace(obj.Salt)
                        ? obj.Salt
                        : BCrypt.Net.BCrypt.GenerateSalt(12);

                    // If salt was provided with hashed password, use as-is; otherwise hash
                    var hashedPassword = !string.IsNullOrWhiteSpace(obj.Salt) && obj.HashedPassword.StartsWith("$2")
                        ? obj.HashedPassword
                        : BCrypt.Net.BCrypt.HashPassword(obj.HashedPassword, salt);

                    var newId = 0;
                    if (MockDb.Players.Count > 0)
                        newId = MockDb.Players.Max(r => r.Id) + 1;
                    existingPlayer = new Player
                    {
                        Id = newId,
                        Name = obj.Name,
                        Email = obj.Email,
                        HashedPassword = hashedPassword,
                        Salt = salt,
                        Roles = obj.Roles,
                        IsEnabled = obj.IsEnabled,
                        SecretQuestion = obj.SecretQuestion,
                        SecretAnswer = obj.SecretAnswer,
                        ContactEmail = obj.ContactEmail,
                        UseGravatar = obj.UseGravatar
                    };
                    MockDb.Players.Add(existingPlayer);
                }
                else
                {
                    existingPlayer.Name = obj.Name;
                    existingPlayer.Roles = obj.Roles;
                    existingPlayer.IsEnabled = obj.IsEnabled;
                    existingPlayer.SecretQuestion = obj.SecretQuestion;
                    existingPlayer.SecretAnswer = obj.SecretAnswer;
                    existingPlayer.ContactEmail = obj.ContactEmail;
                    existingPlayer.UseGravatar = obj.UseGravatar;
                    if (string.IsNullOrWhiteSpace(existingPlayer.Salt))
                        existingPlayer.Salt = BCrypt.Net.BCrypt.GenerateSalt(12);
                    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(obj.HashedPassword, existingPlayer.Salt);
                    if (existingPlayer.HashedPassword != hashedPassword)
                        existingPlayer.HashedPassword = hashedPassword;
                }
                return Task.FromResult(existingPlayer);
            }
        }

        public Task ChangePassword(int id, string oldPassword, string newPassword)
        {
            lock (MockDb.Players)
            {
                var existingPlayer = MockDb.Players.Where(r => r.Id == id).FirstOrDefault()
                    ?? throw new NotFoundException($"{nameof(Player)} {id}");

                if (string.IsNullOrWhiteSpace(existingPlayer.Salt))
                    existingPlayer.Salt = BCrypt.Net.BCrypt.GenerateSalt(12);

                if (!string.IsNullOrWhiteSpace(existingPlayer.HashedPassword))
                {
                    var oldHashedPassword = BCrypt.Net.BCrypt.HashPassword(oldPassword, existingPlayer.Salt);
                    if (existingPlayer.HashedPassword != oldHashedPassword)
                        throw new OperationFailedException($"Old password doesn't match for {nameof(Player)} {id}");
                }

                existingPlayer.HashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword, existingPlayer.Salt);
                return Task.CompletedTask;
            }
        }

        public Task<string?> GetSecretQuestionAsync(string username)
        {
            lock (MockDb.Players)
            {
                var player = MockDb.Players.FirstOrDefault(p => p.Email == username);
                // Return null for unknown user (prevents enumeration)
                return Task.FromResult(player?.SecretQuestion);
            }
        }

        public Task<bool> IsRecoveryLockedOutAsync(string username)
        {
            lock (MockDb.Players)
            {
                var player = MockDb.Players.FirstOrDefault(p => p.Email == username);
                if (player == null) return Task.FromResult(false);

                var isLockedOut = player.RecoveryLockoutUntil.HasValue &&
                                  player.RecoveryLockoutUntil.Value > DateTime.UtcNow;
                return Task.FromResult(isLockedOut);
            }
        }

        public Task<int> GetRemainingRecoveryAttemptsAsync(string username)
        {
            lock (MockDb.Players)
            {
                var player = MockDb.Players.FirstOrDefault(p => p.Email == username);
                if (player == null) return Task.FromResult(3); // Default for unknown (don't reveal)

                const int maxAttempts = 3;
                var remaining = maxAttempts - player.FailedRecoveryAttempts;
                return Task.FromResult(remaining > 0 ? remaining : 0);
            }
        }

        public Task<bool> ValidateSecretAnswerAsync(string username, string answer)
        {
            lock (MockDb.Players)
            {
                var player = MockDb.Players.FirstOrDefault(p => p.Email == username);
                if (player == null) return Task.FromResult(false);

                // Check lockout
                if (player.RecoveryLockoutUntil.HasValue &&
                    player.RecoveryLockoutUntil.Value > DateTime.UtcNow)
                {
                    return Task.FromResult(false);
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

                return Task.FromResult(isCorrect);
            }
        }

        public Task ResetPasswordAsync(string username, string newPassword)
        {
            lock (MockDb.Players)
            {
                var player = MockDb.Players.FirstOrDefault(p => p.Email == username);
                if (player == null)
                    throw new NotFoundException($"Player {username}");

                // Hash new password with BCrypt
                var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
                player.Salt = salt;
                player.HashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword, salt);

                // Clear recovery state
                player.FailedRecoveryAttempts = 0;
                player.RecoveryLockoutUntil = null;

                return Task.CompletedTask;
            }
        }

        public Task<int> CountEnabledAdminsAsync()
        {
            lock (MockDb.Players)
            {
                var count = MockDb.Players.Count(p =>
                    p.IsEnabled &&
                    p.Roles != null &&
                    p.Roles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                        .Contains("Administrator", StringComparer.OrdinalIgnoreCase));
                return Task.FromResult(count);
            }
        }
    }
}
