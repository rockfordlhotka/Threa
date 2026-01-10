using System;
using System.Threading.Tasks;
using Csla;
using Threa.Dal;

namespace GameMechanics.Items;

[Serializable]
public class ItemTemplateList : ReadOnlyListBase<ItemTemplateList, ItemTemplateInfo>
{
    [Fetch]
    private async Task Fetch([Inject] IItemTemplateDal dal, [Inject] IChildDataPortal<ItemTemplateInfo> childPortal)
    {
        var templates = await dal.GetAllTemplatesAsync();
        using (LoadListMode)
        {
            foreach (var template in templates)
            {
                Add(childPortal.FetchChild(template));
            }
        }
    }
}
