using System.Collections.Generic;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal;

public interface ICharacterDal
{
    Character GetBlank();
    Task<List<Character>> GetCharactersAsync(int playerId);
    Task<Character> GetCharacterAsync(int id);
    Task<Character> SaveCharacterAsync(Character character);
    Task DeleteCharacterAsync(int id);
}