using System;
using System.Collections.Generic;
using Threa.Dal.Dto;

namespace Threa.Dal.MockDb
{
  public static class MockDb
  {
    public static readonly List<IPlayer> Players = new List<IPlayer>
    { 
      new Player { Id = Guid.NewGuid().ToString(), Name = "Rocky", Email = "illiante@yahoo.com" }
    };

    public static readonly List<ICharacter> Characters = new List<ICharacter>
    {
      new Character
      {
        Id = Guid.NewGuid().ToString(),
        PlayerEmail = "illiante@yahoo.com",
        Name = "Illiante",
        Species = "Human"
      }
    };
  }
}
