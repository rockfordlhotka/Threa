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
      [Inject] ITableDal tableDal,
      [Inject] IChildDataPortal<CharacterInfo> characterPortal)
    {
      var items = await dal.GetCharactersAsync(playerId);
      using (LoadListMode)
      {
        foreach (var item in items)
        {
          var table = await tableDal.GetTableForCharacterAsync(item.Id);
          Add(characterPortal.FetchChild(item, table));
        }
      }
    }
  }
}
