using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal.MockDb;

/// <summary>
/// Mock database implementation for species data access.
/// </summary>
public class SpeciesDal : ISpeciesDal
{
    public Task<List<Species>> GetAllSpeciesAsync()
    {
        return Task.FromResult(MockDb.Species.ToList());
    }

    public Task<Species> GetSpeciesAsync(string id)
    {
        var species = MockDb.Species.FirstOrDefault(s => s.Id == id);
        if (species == null)
            throw new NotFoundException($"Species '{id}' not found");
        return Task.FromResult(species);
    }
}
