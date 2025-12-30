using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal.MockDb;

/// <summary>
/// Mock database implementation for spell definitions.
/// </summary>
public class SpellDefinitionDal : ISpellDefinitionDal
{
    public Task<SpellDefinition?> GetSpellBySkillIdAsync(string skillId)
    {
        var spell = MockDb.SpellDefinitions.FirstOrDefault(s => s.SkillId == skillId);
        return Task.FromResult(spell);
    }

    public Task<List<SpellDefinition>> GetSpellsBySchoolAsync(MagicSchool school)
    {
        var spells = MockDb.SpellDefinitions
            .Where(s => s.MagicSchool == school)
            .ToList();
        return Task.FromResult(spells);
    }

    public Task<List<SpellDefinition>> GetAllSpellsAsync()
    {
        return Task.FromResult(MockDb.SpellDefinitions.ToList());
    }

    public Task SaveSpellAsync(SpellDefinition spell)
    {
        var existing = MockDb.SpellDefinitions.FirstOrDefault(s => s.SkillId == spell.SkillId);
        if (existing != null)
        {
            MockDb.SpellDefinitions.Remove(existing);
        }
        MockDb.SpellDefinitions.Add(spell);
        return Task.CompletedTask;
    }

    public Task<bool> IsSpellAsync(string skillId)
    {
        var isSpell = MockDb.SpellDefinitions.Any(s => s.SkillId == skillId);
        return Task.FromResult(isSpell);
    }
}
