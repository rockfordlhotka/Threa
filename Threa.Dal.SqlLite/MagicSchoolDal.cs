using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal;
using Threa.Dal.Dto;

namespace Threa.Dal.Sqlite;

public class MagicSchoolDal : IMagicSchoolDal
{
    private static readonly List<MagicSchoolDefinition> _schools = GetHardcodedSchools();

    public Task<List<MagicSchoolDefinition>> GetAllSchoolsAsync()
    {
        // TODO: Implement SQLite persistence for magic schools
        return Task.FromResult(_schools);
    }

    public Task<MagicSchoolDefinition?> GetSchoolAsync(string id)
    {
        // TODO: Implement SQLite persistence for magic schools
        return Task.FromResult(_schools.FirstOrDefault(s => s.Id == id));
    }

    public Task<MagicSchoolDefinition> SaveSchoolAsync(MagicSchoolDefinition school)
    {
        // TODO: Implement SQLite persistence for magic schools
        var existing = _schools.FirstOrDefault(s => s.Id == school.Id);
        if (existing != null)
        {
            _schools.Remove(existing);
        }
        _schools.Add(school);
        return Task.FromResult(school);
    }

    public Task DeleteSchoolAsync(string id)
    {
        // TODO: Implement SQLite persistence for magic schools
        var existing = _schools.FirstOrDefault(s => s.Id == id);
        if (existing != null)
        {
            if (existing.IsCore)
            {
                throw new InvalidOperationException("Cannot delete core magic schools.");
            }
            _schools.Remove(existing);
        }
        return Task.CompletedTask;
    }

    private static List<MagicSchoolDefinition> GetHardcodedSchools()
    {
        return
        [
            new MagicSchoolDefinition { Id = "fire", Name = "Fire", Description = "Fire magic channels the raw power of flame and heat.", ShortDescription = "Offensive magic of flame and heat", ColorCode = "#FF4500", IsActive = true, IsCore = true, ManaSkillId = "fire-mana", DisplayOrder = 1, TypicalSpellTypes = "Offensive damage, area control, illumination" },
            new MagicSchoolDefinition { Id = "water", Name = "Water", Description = "Water magic encompasses the fluid nature of water and ice.", ShortDescription = "Ice, healing, and purification", ColorCode = "#4169E1", IsActive = true, IsCore = true, ManaSkillId = "water-mana", DisplayOrder = 2, TypicalSpellTypes = "Healing, ice attacks, purification" },
            new MagicSchoolDefinition { Id = "light", Name = "Light", Description = "Light magic harnesses the power of illumination and truth.", ShortDescription = "Illumination, truth, and protection", ColorCode = "#FFD700", IsActive = true, IsCore = true, ManaSkillId = "light-mana", DisplayOrder = 3, TypicalSpellTypes = "Illumination, truth detection, protection" },
            new MagicSchoolDefinition { Id = "life", Name = "Life", Description = "Life magic taps into the vital force of living things.", ShortDescription = "Healing, growth, and vitality", ColorCode = "#32CD32", IsActive = true, IsCore = true, ManaSkillId = "life-mana", DisplayOrder = 4, TypicalSpellTypes = "Healing, growth enhancement, vitality buffs" }
        ];
    }
}
