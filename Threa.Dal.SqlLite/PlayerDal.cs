using Threa.Dal.Dto;

namespace Threa.Dal.Sqlite;

public class PlayerDal : IPlayerDal
{
    public Task DeletePlayerAsync(int id)
    {
        return Task.CompletedTask;
    }

    public Task<Player> GetPlayerAsync(int id)
    {
        return Task.FromResult(new Player { Id = 42, Name = "Rocky", Email = "rocky@lhotka.net" });
    }

    public Task<Player> GetPlayerByEmailAsync(string email)
    {
        return Task.FromResult(new Player { Id = 42, Name = "Rocky", Email = "rocky@lhotka.net" });
    }

    public Task<Player> SavePlayerAsync(Player obj)
    {
        return Task.FromResult(new Player { Id = 42, Name = "Rocky", Email = "rocky@lhotka.net" });
    }
}
