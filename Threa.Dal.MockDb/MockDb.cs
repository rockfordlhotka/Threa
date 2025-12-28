using System.Collections.Generic;
using Threa.Dal.Dto;

namespace Threa.Dal.MockDb;

public static class MockDb
{
    public static readonly List<Player> Players =
    [
      new Player { Id = 42, Name = "Rocky", Email = "rocky@lhotka.net" }
    ];

    public static readonly List<string> Images = [];

    /// <summary>
    /// Species reference data with attribute modifiers per design spec.
    /// </summary>
    public static readonly List<Species> Species =
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
                new SpeciesAttributeModifier { AttributeName = "PHY", Modifier = -1 }
            ]
        }
    ];

    public static readonly List<Character> Characters =
    [
      new Character
      {
        Id = 1,
        PlayerId = 42,
        Name = "Illiante",
        Species = "Human",
        AttributeList = new List<CharacterAttribute>
        {
            new() { Name = "STR", BaseValue = 10 },
            new() { Name = "DEX", BaseValue = 10 },
            new() { Name = "END", BaseValue = 10 },
            new() { Name = "INT", BaseValue = 10 },
            new() { Name = "ITT", BaseValue = 10 },
            new() { Name = "WIS", BaseValue = 10 },
            new() { Name = "PHY", BaseValue = 10 },
            new() { Name = "SOC", BaseValue = 10 },
            new() { Name = "PSY", BaseValue = 10 }
        },
        FatBaseValue = 10,
        FatValue = 10,
        VitBaseValue = 10,
        VitValue = 10,
        ActionPointRecovery = 1,
        ActionPointMax = 5,
        ActionPointAvailable = 5,
        DamageClass = 1,
        Description = "A human with a sword",
        HairDescription = "Long and black",
        Height = "5' 6\"",
        SkinDescription = "Fair",
        Weight = "150 lbs"
      }
    ];
}
