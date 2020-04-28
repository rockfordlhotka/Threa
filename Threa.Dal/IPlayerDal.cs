using System;
using System.Collections.Generic;

namespace Threa.Dal
{
  public interface IPlayerDal
  {
    Player GetPlayer(string id);
    void SavePlayer(Player player);
    void DeletePlayer(string id);
    List<Character> GetCharacters(string playerId);
  }
}
