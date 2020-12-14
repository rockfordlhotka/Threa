using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal.MockDb
{
  public class CharacterDal : ICharacterDal
  {
    public ICharacter GetBlank()
    {
      return new Character();
    }

    public Task DeleteCharacterAsync(int id)
    {
      var character = MockDb.Characters.Where(r => r.Id == id).FirstOrDefault();
      if (character == null)
        throw new NotFoundException(nameof(character));
      MockDb.Characters.Remove(character);
      return Task.CompletedTask;
    }

    public Task<ICharacter> GetCharacterAsync(int id)
    {
      var character = MockDb.Characters.Where(r => r.Id == id).FirstOrDefault();
      if (character == null)
        throw new NotFoundException(nameof(character));
      return Task.FromResult(character);
    }

    public Task<List<ICharacter>> GetCharactersAsync(int playerId)
    {
      var character = MockDb.Characters.Where(r => r.PlayerId == playerId);
      return Task.FromResult(character.ToList());
    }

    public Task<ICharacter> SaveCharacter(ICharacter character)
    {
      if (character.Id == 0)
      {
        character.Id = MockDb.Characters.Max(r => r.Id);
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
