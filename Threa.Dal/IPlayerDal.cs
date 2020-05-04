using System.Collections.Generic;
using System.Threading.Tasks;

namespace Threa.Dal
{
  public interface IPlayerDal
  {
    Task<IPlayer> GetPlayer(string id);
    Task<IPlayer> SavePlayer(IPlayer obj);
    Task DeletePlayer(string id);
    Task<IEnumerable<ICharacter>> GetCharacters(string playerId);
  }
}
