using System.Collections.Generic;

namespace Threa.Dal.MockDb
{
  public static class MockDb
  {
    public static readonly List<IPlayer> Players = new List<IPlayer>();
    public static readonly List<ICharacter> Characters = new List<ICharacter>();
  }
}
