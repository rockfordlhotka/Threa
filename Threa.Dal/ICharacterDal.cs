using System.Collections.Generic;
using System.Threading.Tasks;

namespace Threa.Dal
{
  public interface ICharacterDal
  {
    ICharacter GetBlank();
    Task<List<ICharacter>> GetCharactersAsync(int playerId);
    Task<ICharacter> GetCharacterAsync(int id);
    Task<ICharacter> SaveCharacter(ICharacter character);
    Task DeleteCharacterAsync(int id);
  }
}
