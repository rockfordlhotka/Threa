using System.Linq;
using System.Collections.Generic;
using Threa.Dal.Dto;

namespace Threa.Dal.MockDb
{
  public static class MockDb
  {
    public static readonly List<IPlayer> Players = new List<IPlayer>
    { 
      new Player { Id = 42, Name = "Rocky", Email = "illiante@yahoo.com" }
    };

    public static readonly List<ICharacter> Characters = new List<ICharacter>
    {
      new Character
      {
        Id = Players.Max(r=>r.Id) + 1,
        Name = "Illiante",
        Species = "Human"
      }
    };
  }
}
