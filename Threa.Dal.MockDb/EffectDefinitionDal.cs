using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal.MockDb;

/// <summary>
/// Mock implementation of IEffectDefinitionDal for development and testing.
/// </summary>
public class EffectDefinitionDal : IEffectDefinitionDal
{
    public Task<List<EffectDefinition>> GetAllDefinitionsAsync()
    {
        var definitions = MockDb.EffectDefinitions.Where(d => d.IsActive).ToList();
        return Task.FromResult(definitions);
    }

    public Task<List<EffectDefinition>> GetDefinitionsByTypeAsync(EffectType effectType)
    {
        var definitions = MockDb.EffectDefinitions
            .Where(d => d.IsActive && d.EffectType == effectType)
            .ToList();
        return Task.FromResult(definitions);
    }

    public Task<EffectDefinition> GetDefinitionAsync(int id)
    {
        var definition = MockDb.EffectDefinitions.FirstOrDefault(d => d.Id == id);
        if (definition == null)
            throw new NotFoundException($"EffectDefinition {id}");
        return Task.FromResult(definition);
    }

    public Task<EffectDefinition?> GetDefinitionByNameAsync(string name)
    {
        var definition = MockDb.EffectDefinitions
            .FirstOrDefault(d => d.IsActive && 
                d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(definition);
    }

    public Task<List<EffectDefinition>> SearchDefinitionsAsync(string searchTerm)
    {
        var term = searchTerm.ToLowerInvariant();
        var definitions = MockDb.EffectDefinitions
            .Where(d => d.IsActive &&
                (d.Name.ToLowerInvariant().Contains(term) ||
                 d.Description.ToLowerInvariant().Contains(term)))
            .ToList();
        return Task.FromResult(definitions);
    }

    public Task<EffectDefinition> SaveDefinitionAsync(EffectDefinition definition)
    {
        if (definition.Id == 0)
        {
            definition.Id = MockDb.EffectDefinitions.Count == 0 ?
                1 : MockDb.EffectDefinitions.Max(d => d.Id) + 1;
            
            // Assign IDs to impacts
            int impactId = MockDb.EffectDefinitions
                .SelectMany(d => d.Impacts)
                .Select(i => i.Id)
                .DefaultIfEmpty(0)
                .Max() + 1;
            foreach (var impact in definition.Impacts)
            {
                if (impact.Id == 0)
                    impact.Id = impactId++;
                impact.EffectDefinitionId = definition.Id;
            }
            
            MockDb.EffectDefinitions.Add(definition);
        }
        else
        {
            var existing = MockDb.EffectDefinitions.FirstOrDefault(d => d.Id == definition.Id);
            if (existing == null)
                throw new NotFoundException($"EffectDefinition {definition.Id}");
            MockDb.EffectDefinitions.Remove(existing);
            MockDb.EffectDefinitions.Add(definition);
        }
        return Task.FromResult(definition);
    }

    public Task DeactivateDefinitionAsync(int id)
    {
        var definition = MockDb.EffectDefinitions.FirstOrDefault(d => d.Id == id);
        if (definition == null)
            throw new NotFoundException($"EffectDefinition {id}");
        definition.IsActive = false;
        return Task.CompletedTask;
    }
}
