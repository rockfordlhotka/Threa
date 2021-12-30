using Csla;
using System;
using System.Threading.Tasks;
using Threa.Dal;

namespace GameMechanics.Player
{
  [Serializable]
  public class CharacterList : ReadOnlyListBase<CharacterList, CharacterInfo>
  {
    [Fetch]
    private async Task Fetch(int playerId, 
      [Inject] ICharacterDal dal, 
      [Inject] IChildDataPortal<CharacterInfo> characterPortal)
    {
      var items = await dal.GetCharactersAsync(playerId);
      using (LoadListMode)
      {
        foreach (var item in items)
          Add(characterPortal.FetchChild(item));
      }
    }
  }
}
