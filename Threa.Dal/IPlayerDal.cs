using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal;

public interface IPlayerDal
{
    Task<Player?> GetPlayerAsync(int id);
    Task<Player> GetPlayerByEmailAsync(string email, string password);
    Task<Player> SavePlayerAsync(Player obj);
    Task ChangePassword(int id, string oldPassword, string newPassword);
    Task DeletePlayerAsync(int id);
}