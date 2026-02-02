using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal;

public interface ICharacterDal
{
    Character GetBlank();
    Task<List<Character>> GetCharactersAsync(int playerId);
    Task<List<Character>> GetAllCharactersAsync();
    Task<List<Character>> GetNpcTemplatesAsync();
    Task<List<Character>> GetTableNpcsAsync(Guid tableId);
    Task<Character> GetCharacterAsync(int id);
    Task<Character> SaveCharacterAsync(Character character);
    Task DeleteCharacterAsync(int id);
}