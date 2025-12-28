using System.Collections.Generic;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal;

/// <summary>
/// Data access layer for species reference data.
/// </summary>
public interface ISpeciesDal
{
    /// <summary>
    /// Gets all available species.
    /// </summary>
    Task<List<Species>> GetAllSpeciesAsync();

    /// <summary>
    /// Gets a specific species by ID.
    /// </summary>
    /// <param name="id">The species ID.</param>
    Task<Species> GetSpeciesAsync(string id);
}
