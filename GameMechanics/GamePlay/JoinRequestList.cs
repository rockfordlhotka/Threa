using Csla;
using System;
using System.Threading.Tasks;
using Threa.Dal;

namespace GameMechanics.GamePlay;

/// <summary>
/// Read-only list of join requests.
/// Can be fetched by player ID (for player's pending requests view) or by table ID (for GM review).
/// </summary>
[Serializable]
public class JoinRequestList : ReadOnlyListBase<JoinRequestList, JoinRequestInfo>
{
    /// <summary>
    /// Fetch all non-denied requests for a player.
    /// Used for "My Join Requests" view showing pending and approved requests.
    /// </summary>
    [Fetch]
    private async Task FetchForPlayer(
        int playerId,
        [Inject] IJoinRequestDal joinRequestDal,
        [Inject] ICharacterDal characterDal,
        [Inject] ITableDal tableDal,
        [Inject] IChildDataPortal<JoinRequestInfo> portal)
    {
        var requests = await joinRequestDal.GetRequestsByPlayerAsync(playerId);
        using (LoadListMode)
        {
            foreach (var request in requests)
            {
                var character = await characterDal.GetCharacterAsync(request.CharacterId);
                Threa.Dal.Dto.GameTable? table = null;
                try { table = await tableDal.GetTableAsync(request.TableId); } catch { }
                Add(portal.FetchChild(request, character, table));
            }
        }
    }

    /// <summary>
    /// Fetch pending requests for a table (GM view).
    /// Used for GM to review and approve/deny requests.
    /// </summary>
    [Fetch]
    private async Task FetchForTable(
        Guid tableId,
        [Inject] IJoinRequestDal joinRequestDal,
        [Inject] ICharacterDal characterDal,
        [Inject] ITableDal tableDal,
        [Inject] IChildDataPortal<JoinRequestInfo> portal)
    {
        var requests = await joinRequestDal.GetPendingRequestsForTableAsync(tableId);
        var table = await tableDal.GetTableAsync(tableId);
        using (LoadListMode)
        {
            foreach (var request in requests)
            {
                var character = await characterDal.GetCharacterAsync(request.CharacterId);
                Add(portal.FetchChild(request, character, table));
            }
        }
    }
}
