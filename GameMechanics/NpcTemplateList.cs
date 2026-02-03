using System;
using System.Threading.Tasks;
using Csla;
using Threa.Dal;

namespace GameMechanics;

/// <summary>
/// Read-only list of NPC templates for the library browser.
/// Fetches all NPC templates via the DAL for display in the UI.
/// </summary>
[Serializable]
public class NpcTemplateList : ReadOnlyListBase<NpcTemplateList, NpcTemplateInfo>
{
    [Fetch]
    private async Task Fetch([Inject] ICharacterDal dal, [Inject] IChildDataPortal<NpcTemplateInfo> childPortal)
    {
        var templates = await dal.GetNpcTemplatesAsync();
        using (LoadListMode)
        {
            foreach (var template in templates)
            {
                Add(childPortal.FetchChild(template));
            }
        }
    }
}
