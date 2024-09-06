using System.Collections.Generic;
using Threa.Dal.Dto;

namespace Threa.Dal.MockDb
{
  public static class MockDb
  {
    public static readonly List<Player> Players = new()
    { 
      new Player { Id = 42, Name = "Rocky", Email = "rocky@lhotka.net" }
    };

    public static readonly List<Character> Characters = new()
    {
      new Character
      {
        Id = 1,
        PlayerId = 42,
        Name = "Illiante",
        Species = "Human"
      }
    };
  }
}
