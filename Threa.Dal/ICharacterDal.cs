using System.Collections.Generic;
using System.Threading.Tasks;

namespace Threa.Dal
{
  public interface ICharacterDal
  {
    ICharacter GetBlank();
    Task<List<ICharacter>> GetCharactersAsync(string playerEmail);
    Task<ICharacter> GetCharacterAsync(string id);
    Task<ICharacter> SaveCharacter(ICharacter character);
    Task DeleteCharacter(string id);
  }
}
