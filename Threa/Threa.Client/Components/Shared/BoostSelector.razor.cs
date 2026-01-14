using Microsoft.AspNetCore.Components;

namespace Threa.Client.Components.Shared;

public class BoostSelectorBase : ComponentBase
{
    [Parameter] public int MaxAP { get; set; }
    [Parameter] public int MaxFAT { get; set; }
    [Parameter] public EventCallback<int> OnBoostChanged { get; set; }
    [Parameter] public EventCallback<(int ApBoost, int FatBoost)> OnBoostCostChanged { get; set; }

    protected int ApBoost { get; set; }
    protected int FatBoost { get; set; }
    // Per COMBAT_SYSTEM.md: 1 AP = +1 AS, 1 FAT = +1 AS (equal value)
    protected int TotalBoost => ApBoost + FatBoost;

    protected async Task IncrementApBoost()
    {
        if (ApBoost < MaxAP)
        {
            ApBoost++;
            await NotifyBoostChanged();
        }
    }

    protected async Task DecrementApBoost()
    {
        if (ApBoost > 0)
        {
            ApBoost--;
            await NotifyBoostChanged();
        }
    }

    protected async Task IncrementFatBoost()
    {
        if (FatBoost < MaxFAT)
        {
            FatBoost++;
            await NotifyBoostChanged();
        }
    }

    protected async Task DecrementFatBoost()
    {
        if (FatBoost > 0)
        {
            FatBoost--;
            await NotifyBoostChanged();
        }
    }

    private async Task NotifyBoostChanged()
    {
        await OnBoostChanged.InvokeAsync(TotalBoost);
        await OnBoostCostChanged.InvokeAsync((ApBoost, FatBoost));
    }
}
