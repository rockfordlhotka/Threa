using Microsoft.AspNetCore.Components;

namespace Threa.Client.Components.Shared;

public class BoostSelectorBase : ComponentBase
{
    /// <summary>
    /// Available AP after base action cost
    /// </summary>
    [Parameter] public int AvailableAP { get; set; }

    /// <summary>
    /// Available FAT after base action cost
    /// </summary>
    [Parameter] public int AvailableFAT { get; set; }

    /// <summary>
    /// Cost type for boosts - matches the action's cost type
    /// </summary>
    [Parameter] public ActionCostType CostType { get; set; } = ActionCostType.OneAPOneFat;

    /// <summary>
    /// Callback when boost amount changes - passes the total +AS bonus
    /// </summary>
    [Parameter] public EventCallback<int> OnBoostChanged { get; set; }

    /// <summary>
    /// Callback with detailed cost breakdown - (TotalBoost, APCost, FATCost)
    /// </summary>
    [Parameter] public EventCallback<(int Boost, int APCost, int FATCost)> OnBoostCostChanged { get; set; }

    protected int Boost { get; set; }

    /// <summary>
    /// Maximum boost based on cost type and available resources.
    /// Standard (1 AP + 1 FAT per boost): limited by min(AP, FAT)
    /// Fatigue-Free (2 AP per boost): limited by AP / 2
    /// </summary>
    protected int MaxBoost => CostType switch
    {
        ActionCostType.OneAPOneFat => Math.Min(AvailableAP, AvailableFAT),
        ActionCostType.TwoAP => AvailableAP / 2,
        _ => 0
    };

    /// <summary>
    /// AP cost for current boost amount
    /// </summary>
    protected int APCost => CostType switch
    {
        ActionCostType.OneAPOneFat => Boost,
        ActionCostType.TwoAP => Boost * 2,
        _ => 0
    };

    /// <summary>
    /// FAT cost for current boost amount
    /// </summary>
    protected int FATCost => CostType switch
    {
        ActionCostType.OneAPOneFat => Boost,
        ActionCostType.TwoAP => 0,
        _ => 0
    };

    protected string CostPerBoostDisplay => CostType switch
    {
        ActionCostType.OneAPOneFat => "1 AP + 1 FAT",
        ActionCostType.TwoAP => "2 AP",
        _ => "Unknown"
    };

    protected override void OnParametersSet()
    {
        // Reset boost if it exceeds new max (e.g., cost type changed)
        if (Boost > MaxBoost)
        {
            Boost = MaxBoost;
            _ = NotifyBoostChanged();
        }
    }

    protected async Task IncrementBoost()
    {
        if (Boost < MaxBoost)
        {
            Boost++;
            await NotifyBoostChanged();
        }
    }

    protected async Task DecrementBoost()
    {
        if (Boost > 0)
        {
            Boost--;
            await NotifyBoostChanged();
        }
    }

    private async Task NotifyBoostChanged()
    {
        await OnBoostChanged.InvokeAsync(Boost);
        await OnBoostCostChanged.InvokeAsync((Boost, APCost, FATCost));
    }
}
