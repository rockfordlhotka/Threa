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
                // Character or table may have been deleted after the join request was created.
                // Skip orphaned requests rather than failing the entire list.
                Threa.Dal.Dto.Character? character;
                Threa.Dal.Dto.GameTable? table;
                try
                {
                    character = await characterDal.GetCharacterAsync(request.CharacterId);
                }
                catch (NotFoundException)
                {
                    continue; // Character deleted, skip this request
                }
                try
                {
                    table = await tableDal.GetTableAsync(request.TableId);
                }
                catch (NotFoundException)
                {
                    continue; // Table deleted, skip this request
                }
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
        // If table doesn't exist, let the NotFoundException propagate - 
        // caller asked for a specific table, they should know it's gone.
        var table = await tableDal.GetTableAsync(tableId);
        using (LoadListMode)
        {
            foreach (var request in requests)
            {
                // Character may have been deleted after the join request was created.
                // Skip orphaned requests rather than failing the entire list.
                Threa.Dal.Dto.Character? character;
                try
                {
                    character = await characterDal.GetCharacterAsync(request.CharacterId);
                }
                catch (NotFoundException)
                {
                    continue; // Character deleted, skip this request
                }
                Add(portal.FetchChild(request, character, table));
            }
        }
    }
}
