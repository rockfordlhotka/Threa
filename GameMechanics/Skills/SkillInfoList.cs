using System;
using System.Threading.Tasks;
using Csla;
using Threa.Dal;

namespace GameMechanics.Skills;

[Serializable]
public class SkillInfoList : ReadOnlyListBase<SkillInfoList, SkillInfo>
{
    [Fetch]
    private async Task Fetch([Inject] ISkillDal dal, [Inject] IChildDataPortal<SkillInfo> childPortal)
    {
        var skills = await dal.GetAllSkillsAsync();
        using (LoadListMode)
        {
            foreach (var skill in skills)
            {
                Add(childPortal.FetchChild(skill));
            }
        }
    }
}
