using Csla;
using System;
using System.Threading.Tasks;
using Threa.Dal;

namespace GameMechanics.GamePlay;

/// <summary>
/// Read-only list of active game tables.
/// </summary>
[Serializable]
public class TableList : ReadOnlyListBase<TableList, TableInfo>
{
    /// <summary>
    /// Fetches all active tables (status != Ended).
    /// </summary>
    [Fetch]
    private async Task FetchAsync(
        [Inject] ITableDal dal,
        [Inject] IChildDataPortal<TableInfo> tablePortal)
    {
        var items = await dal.GetActiveTablesAsync();
        using (LoadListMode)
        {
            foreach (var item in items)
                Add(tablePortal.FetchChild(item));
        }
    }

    /// <summary>
    /// Fetches tables for a specific game master.
    /// </summary>
    [Fetch]
    private async Task FetchAsync(int gameMasterId,
        [Inject] ITableDal dal,
        [Inject] IChildDataPortal<TableInfo> tablePortal)
    {
        var items = await dal.GetTablesByGmAsync(gameMasterId);
        using (LoadListMode)
        {
            foreach (var item in items)
                Add(tablePortal.FetchChild(item));
        }
    }
}
