using System.Collections.Generic;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal;

/// <summary>
/// Data access layer for magic school definitions.
/// </summary>
public interface IMagicSchoolDal
{
    /// <summary>
    /// Gets all magic school definitions.
    /// </summary>
    Task<List<MagicSchoolDefinition>> GetAllSchoolsAsync();

    /// <summary>
    /// Gets a specific magic school by ID.
    /// </summary>
    Task<MagicSchoolDefinition?> GetSchoolAsync(string id);

    /// <summary>
    /// Saves a magic school definition (insert or update).
    /// </summary>
    Task<MagicSchoolDefinition> SaveSchoolAsync(MagicSchoolDefinition school);

    /// <summary>
    /// Deletes a magic school definition.
    /// Cannot delete core schools.
    /// </summary>
    Task DeleteSchoolAsync(string id);
}
