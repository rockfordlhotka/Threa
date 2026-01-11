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
        var species = MockDb.Species ?? new List<Species>();
        return Task.FromResult(species.ToList());
    }

    public Task<Species> GetSpeciesAsync(string id)
    {
        var species = MockDb.Species?.FirstOrDefault(s => s.Id == id);
        if (species == null)
            throw new NotFoundException($"Species '{id}' not found");
        return Task.FromResult(species);
    }

    public Task<Species> SaveSpeciesAsync(Species species)
    {
        if (MockDb.Species == null)
            throw new InvalidOperationException("Species collection is not initialized");
            
        var existing = MockDb.Species.FirstOrDefault(s => s.Id == species.Id);
        if (existing != null)
        {
            MockDb.Species.Remove(existing);
        }
        MockDb.Species.Add(species);
        return Task.FromResult(species);
    }

    public Task DeleteSpeciesAsync(string id)
    {
        if (id.Equals("Human", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Cannot delete the Human species.");
        }

        if (MockDb.Species != null)
        {
            var existing = MockDb.Species.FirstOrDefault(s => s.Id == id);
            if (existing != null)
            {
                MockDb.Species.Remove(existing);
            }
        }
        return Task.CompletedTask;
    }
}

