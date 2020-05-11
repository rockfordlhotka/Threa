using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Threa.Dal.MockDb
{
  public class CharacterDal : ICharacterDal
  {
    public Task DeleteCharacter(string id)
    {
      var character = MockDb.Characters.Where(r => r.Id == id).FirstOrDefault();
      if (character == null)
        throw new NotFoundException(nameof(character));
      MockDb.Characters.Remove(character);
      return Task.CompletedTask;
    }

    public Task<ICharacter> GetCharacter(string id)
    {
      var character = MockDb.Characters.Where(r => r.Id == id).FirstOrDefault();
      if (character == null)
        throw new NotFoundException(nameof(character));
      return Task.FromResult(character);
    }

    public Task<List<ICharacter>> GetCharacters(string playerEmail)
    {
      var character = MockDb.Characters.Where(r => r.PlayerEmail == playerEmail);
      return Task.FromResult(character.ToList());
    }

    public Task<ICharacter> SaveCharacter(ICharacter character)
    {
      if (string.IsNullOrWhiteSpace(character.Id))
      {
        character.Id = Guid.NewGuid().ToString();
        MockDb.Characters.Add(character);
      }
      else
      {
        ICharacter existing = MockDb.Characters.Where(r => r.Id == character.Id).FirstOrDefault();
        if (existing == null)
          throw new NotFoundException(nameof(character));
        MockDb.Characters.Remove(existing);
        MockDb.Characters.Add(character);
      }
      return Task.FromResult(character);
    }
  }
}
