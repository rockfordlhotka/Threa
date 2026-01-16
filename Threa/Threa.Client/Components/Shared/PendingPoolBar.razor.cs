using Microsoft.AspNetCore.Components;

namespace Threa.Client.Components.Shared;

public class PendingPoolBarBase : ComponentBase
{
    [Parameter] public int CurrentValue { get; set; }
    [Parameter] public int MaxValue { get; set; }
    [Parameter] public int PendingDamage { get; set; }
    [Parameter] public int PendingHealing { get; set; }
    [Parameter] public bool ShowLabels { get; set; } = false;
    [Parameter] public string Height { get; set; } = "8px";

    protected double BasePercentage { get; private set; }
    protected double DamagePercentage { get; private set; }
    protected double HealingPercentage { get; private set; }
    protected string TooltipText { get; private set; } = "";

    protected override void OnParametersSet()
    {
        if (MaxValue <= 0)
        {
            BasePercentage = 0;
            DamagePercentage = 0;
            HealingPercentage = 0;
            TooltipText = "Invalid MaxValue";
            return;
        }

        var current = Math.Clamp(CurrentValue, 0, MaxValue);
        var damage = Math.Clamp(PendingDamage, 0, current);
        var healing = Math.Max(0, PendingHealing);

        var valueAfterDamage = current - damage;
        
        BasePercentage = (double)valueAfterDamage / MaxValue * 100;
        DamagePercentage = (double)damage / MaxValue * 100;
        HealingPercentage = (double)Math.Min(healing, MaxValue - current) / MaxValue * 100;

        var futureValue = CurrentValue - PendingDamage + PendingHealing;
        TooltipText = $"Current: {CurrentValue}/{MaxValue} | Pending: -{PendingDamage} +{PendingHealing} | Future: {futureValue}/{MaxValue}";
    }
}
