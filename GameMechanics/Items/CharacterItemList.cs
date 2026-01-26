using System;
using System.Threading.Tasks;
using Csla;
using Threa.Dal;

namespace GameMechanics.Items;

[Serializable]
public class CharacterItemList : ReadOnlyListBase<CharacterItemList, CharacterItemInfo>
{
    [Fetch]
    private async Task Fetch(int characterId,
        [Inject] ICharacterItemDal dal,
        [Inject] IChildDataPortal<CharacterItemInfo> childPortal)
    {
        var items = await dal.GetCharacterItemsAsync(characterId);
        using (LoadListMode)
        {
            foreach (var item in items)
            {
                Add(childPortal.FetchChild(item));
            }
        }
    }
}
