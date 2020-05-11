using System.Collections.Generic;
using System.Threading.Tasks;

namespace Threa.Dal
{
  public interface IPlayerDal
  {
    Task<IPlayer> GetPlayerAsync(string id);
    Task<IPlayer> GetPlayerByEmailAsync(string email);
    Task<IPlayer> SavePlayerAsync(IPlayer obj);
    Task DeletePlayerAsync(string id);
    Task<IEnumerable<ICharacter>> GetCharactersAsync(string playerEmail);
  }
}
