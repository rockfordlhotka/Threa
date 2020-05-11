using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Threa.Dal.MongoDb
{
  public class CharacterDal : ICharacterDal
  {
    public Task DeleteCharacter(string id)
    {
      throw new NotImplementedException();
    }

    public Task<ICharacter> GetCharacter(string id)
    {
      throw new NotImplementedException();
    }

    public Task<List<ICharacter>> GetCharacters(string playerEmail)
    {
      throw new NotImplementedException();
    }

    public Task<ICharacter> SaveCharacter(ICharacter character)
    {
      throw new NotImplementedException();
    }
  }
}
