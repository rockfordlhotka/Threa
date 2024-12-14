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
