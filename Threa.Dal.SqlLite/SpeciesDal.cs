using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal;
using Threa.Dal.Dto;

namespace Threa.Dal.Sqlite;

/// <summary>
/// SQLite implementation for species data access.
/// </summary>
public class SpeciesDal : ISpeciesDal
{
    // TODO: Implement SQLite persistence for species
    // For now, using hardcoded data matching MockDb
    private static readonly List<Species> _species = GetHardcodedSpecies();

    public Task<List<Species>> GetAllSpeciesAsync()
    {
        return Task.FromResult(_species.ToList());
    }

    public Task<Species> GetSpeciesAsync(string id)
    {
        var species = _species.FirstOrDefault(s => s.Id == id);
        if (species == null)
            throw new NotFoundException($"Species '{id}' not found");
        return Task.FromResult(species);
    }

    public Task<Species> SaveSpeciesAsync(Species species)
    {
        var existing = _species.FirstOrDefault(s => s.Id == species.Id);
        if (existing != null)
        {
            _species.Remove(existing);
        }
        _species.Add(species);
        return Task.FromResult(species);
    }

    public Task DeleteSpeciesAsync(string id)
    {
        if (id.Equals("Human", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Cannot delete the Human species.");
        }

        var existing = _species.FirstOrDefault(s => s.Id == id);
        if (existing != null)
        {
            _species.Remove(existing);
        }
        return Task.CompletedTask;
    }

    private static List<Species> GetHardcodedSpecies()
    {
        return
        [
            new Species
            {
                Id = "Human",
                Name = "Human",
                Description = "Humans are the baseline species with no attribute modifiers.",
                AttributeModifiers = []
            },
            new Species
            {
                Id = "Elf",
                Name = "Elf",
                Description = "Elves are intellectual and agile, but physically delicate.",
                AttributeModifiers =
                [
                    new SpeciesAttributeModifier { AttributeName = "INT", Modifier = 1 },
                    new SpeciesAttributeModifier { AttributeName = "STR", Modifier = -1 }
                ]
            },
            new Species
            {
                Id = "Dwarf",
                Name = "Dwarf",
                Description = "Dwarves are strong and resilient, but less agile.",
                AttributeModifiers =
                [
                    new SpeciesAttributeModifier { AttributeName = "STR", Modifier = 1 },
                    new SpeciesAttributeModifier { AttributeName = "DEX", Modifier = -1 }
                ]
            },
            new Species
            {
                Id = "Halfling",
                Name = "Halfling",
                Description = "Halflings are quick and perceptive, but physically weak.",
                AttributeModifiers =
                [
                    new SpeciesAttributeModifier { AttributeName = "DEX", Modifier = 1 },
                    new SpeciesAttributeModifier { AttributeName = "ITT", Modifier = 1 },
                    new SpeciesAttributeModifier { AttributeName = "STR", Modifier = -2 }
                ]
            },
            new Species
            {
                Id = "Orc",
                Name = "Orc",
                Description = "Orcs are physically powerful and enduring, but less intelligent and social.",
                AttributeModifiers =
                [
                    new SpeciesAttributeModifier { AttributeName = "STR", Modifier = 2 },
                    new SpeciesAttributeModifier { AttributeName = "END", Modifier = 1 },
                    new SpeciesAttributeModifier { AttributeName = "INT", Modifier = -1 },
                    new SpeciesAttributeModifier { AttributeName = "SOC", Modifier = -1 }
                ]
            }
        ];
    }
}
