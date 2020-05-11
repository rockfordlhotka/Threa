using System.Collections.Generic;
using System.Threading.Tasks;

namespace Threa.Dal
{
  public interface ICharacterDal
  {
    Task<List<ICharacter>> GetCharacters(string playerEmail);
    Task<ICharacter> GetCharacter(string id);
    Task<ICharacter> SaveCharacter(ICharacter character);
    Task DeleteCharacter(string id);
  }
}
