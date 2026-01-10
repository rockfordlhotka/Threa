using System;
using System.Linq;
using System.Threading.Tasks;
using Csla;
using Threa.Dal;

namespace GameMechanics.Magic;

[Serializable]
public class MagicSchoolInfoList : ReadOnlyListBase<MagicSchoolInfoList, MagicSchoolInfo>
{
    [Fetch]
    private async Task Fetch([Inject] IMagicSchoolDal dal, [Inject] IChildDataPortal<MagicSchoolInfo> childPortal)
    {
        var schools = await dal.GetAllSchoolsAsync();
        using (LoadListMode)
        {
            foreach (var school in schools.OrderBy(s => s.DisplayOrder))
            {
                Add(childPortal.FetchChild(school));
            }
        }
    }
}
