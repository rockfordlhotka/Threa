using Csla;
using System;
using System.Threading.Tasks;
using Threa.Dal;

namespace GameMechanics.GameMaster
{
  [Serializable]
  public class GmCharacterList : ReadOnlyListBase<GmCharacterList, GmCharacterInfo>
  {
    [Fetch]
    private async Task Fetch(
      [Inject] ICharacterDal dal,
      [Inject] IChildDataPortal<GmCharacterInfo> characterPortal)
    {
      var items = await dal.GetAllCharactersAsync();
      using (LoadListMode)
      {
        foreach (var item in items)
          Add(characterPortal.FetchChild(item));
      }
    }
  }
}
