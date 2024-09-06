using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal;

public interface IPlayerDal
{
    Task<Player> GetPlayerAsync(int id);
    Task<Player> GetPlayerByEmailAsync(string email);
    Task<Player> SavePlayerAsync(Player obj);
    Task DeletePlayerAsync(int id);
}