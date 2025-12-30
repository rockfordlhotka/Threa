using System.Collections.Generic;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal;

/// <summary>
/// Data access layer for spell definitions.
/// </summary>
public interface ISpellDefinitionDal
{
    /// <summary>
    /// Gets a spell definition by its skill ID.
    /// </summary>
    /// <param name="skillId">The skill ID of the spell.</param>
    /// <returns>The spell definition, or null if not found.</returns>
    Task<SpellDefinition?> GetSpellBySkillIdAsync(string skillId);

    /// <summary>
    /// Gets all spells for a magic school.
    /// </summary>
    /// <param name="school">The magic school.</param>
    /// <returns>List of spell definitions in that school.</returns>
    Task<List<SpellDefinition>> GetSpellsBySchoolAsync(MagicSchool school);

    /// <summary>
    /// Gets all spell definitions.
    /// </summary>
    /// <returns>List of all spell definitions.</returns>
    Task<List<SpellDefinition>> GetAllSpellsAsync();

    /// <summary>
    /// Creates or updates a spell definition.
    /// </summary>
    /// <param name="spell">The spell definition to save.</param>
    Task SaveSpellAsync(SpellDefinition spell);

    /// <summary>
    /// Checks if a skill ID corresponds to a spell.
    /// </summary>
    /// <param name="skillId">The skill ID to check.</param>
    /// <returns>True if the skill is a spell.</returns>
    Task<bool> IsSpellAsync(string skillId);
}
