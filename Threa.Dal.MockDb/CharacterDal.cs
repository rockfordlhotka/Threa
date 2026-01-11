using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal.MockDb
{
  public class CharacterDal : ICharacterDal
  {
    public Character GetBlank()
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

    public Task<Character> GetCharacterAsync(int id)
    {
      var character = MockDb.Characters.Where(r => r.Id == id).FirstOrDefault();
      if (character == null)
        throw new NotFoundException(nameof(character));
      return Task.FromResult(character);
    }

    public Task<List<Character>> GetCharactersAsync(int playerId)
    {
      var character = MockDb.Characters.Where(r => r.PlayerId == playerId);
      return Task.FromResult(character.ToList());
    }

    public Task<List<Character>> GetAllCharactersAsync()
    {
      return Task.FromResult(MockDb.Characters.ToList());
    }

    public Task<Character> SaveCharacterAsync(Character character)
    {
      lock (MockDb.Characters)
      {
        if (character.Id == 0)
        {
          character.Id = MockDb.Characters.Count == 0 ? 
            1 : MockDb.Characters.Max(r => r.Id) + 1;
          MockDb.Characters.Add(character);
        }
        else
        {
          Character? existing = MockDb.Characters.Where(r => r.Id == character.Id).FirstOrDefault();
          if (existing == null)
            throw new NotFoundException(nameof(character));
          
          // Only remove/add if they're different objects
          // If they're the same reference, the object is already modified in place
          if (!ReferenceEquals(existing, character))
          {
            MockDb.Characters.Remove(existing);
            MockDb.Characters.Add(character);
          }
        }
      }
      return Task.FromResult(character);
    }
  }
}
