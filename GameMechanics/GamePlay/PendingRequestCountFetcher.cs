using Csla;
using System;
using System.Threading.Tasks;
using Threa.Dal;

namespace GameMechanics.GamePlay;

/// <summary>
/// Command to fetch the count of pending join requests for a table.
/// Used for badge display showing "3 pending" on campaign list.
/// </summary>
[Serializable]
public class PendingRequestCountFetcher : CommandBase<PendingRequestCountFetcher>
{
    public static readonly PropertyInfo<int> CountProperty = RegisterProperty<int>(nameof(Count));
    public int Count
    {
        get => ReadProperty(CountProperty);
        private set => LoadProperty(CountProperty, value);
    }

    [Execute]
    private async Task ExecuteAsync(
        Guid tableId,
        [Inject] IJoinRequestDal joinRequestDal)
    {
        var count = await joinRequestDal.GetPendingCountForTableAsync(tableId);
        LoadProperty(CountProperty, count);
    }
}
