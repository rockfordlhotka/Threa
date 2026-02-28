using Microsoft.AspNetCore.Components;
using GameMechanics;

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
    /// Whether the selector is read-only
    /// </summary>
    [Parameter] public bool IsReadOnly { get; set; }

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
    /// Maximum boost based on total available resources.
    /// Per game design: 1 AP or 1 FAT = +1 AS; AP and FAT can be mixed.
    /// </summary>
    protected int MaxBoost => AvailableAP + AvailableFAT;

    /// <summary>
    /// AP cost for current boost amount (AP spent first, then FAT)
    /// </summary>
    protected int APCost => Math.Min(Boost, AvailableAP);

    /// <summary>
    /// FAT cost for current boost amount (FAT used only after AP is exhausted)
    /// </summary>
    protected int FATCost => Math.Max(0, Boost - AvailableAP);

    protected string CostPerBoostDisplay => "1 AP or 1 FAT";

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
        if (IsReadOnly || Boost >= MaxBoost) return;
        Boost++;
        await NotifyBoostChanged();
    }

    protected async Task DecrementBoost()
    {
        if (IsReadOnly || Boost <= 0) return;
        Boost--;
        await NotifyBoostChanged();
    }

    private async Task NotifyBoostChanged()
    {
        await OnBoostChanged.InvokeAsync(Boost);
        await OnBoostCostChanged.InvokeAsync((Boost, APCost, FATCost));
    }
}
