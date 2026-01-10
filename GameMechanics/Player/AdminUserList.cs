using System;
using System.Threading.Tasks;
using Csla;
using Threa.Dal;

namespace GameMechanics.Player;

[Serializable]
public class AdminUserList : ReadOnlyListBase<AdminUserList, AdminUserInfo>
{
    [Fetch]
    private async Task Fetch([Inject] IPlayerDal dal, [Inject] IChildDataPortal<AdminUserInfo> childPortal)
    {
        var players = await dal.GetAllPlayersAsync();
        using (LoadListMode)
        {
            foreach (var player in players)
            {
                Add(childPortal.FetchChild(player));
            }
        }
    }
}
