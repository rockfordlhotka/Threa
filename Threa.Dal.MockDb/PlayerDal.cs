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
                    var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
                    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(obj.HashedPassword, salt);
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
                        Roles = obj.Roles
                    };
                    MockDb.Players.Add(existingPlayer);
                }
                else
                {
                    existingPlayer.Name = obj.Name;
                    existingPlayer.Roles = obj.Roles;
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
    }
}
