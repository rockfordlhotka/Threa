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
    protected string BarColorClass { get; private set; } = "health-full";
    protected int OverhealAmount { get; private set; }

    protected override void OnParametersSet()
    {
        Console.WriteLine($"[PendingPoolBar] OnParametersSet: Current={CurrentValue}, Max={MaxValue}, PendingDmg={PendingDamage}, PendingHeal={PendingHealing}");
        if (MaxValue <= 0)
        {
            BasePercentage = 0;
            DamagePercentage = 0;
            HealingPercentage = 0;
            TooltipText = "Invalid MaxValue";
            BarColorClass = "health-full";
            OverhealAmount = 0;
            return;
        }

        var current = Math.Clamp(CurrentValue, 0, MaxValue);
        var damage = Math.Clamp(PendingDamage, 0, current);
        var healing = Math.Max(0, PendingHealing);

        var valueAfterDamage = current - damage;
        var effectiveValue = current - damage + healing;

        // Calculate overheal amount (when healing exceeds max)
        OverhealAmount = Math.Max(0, effectiveValue - MaxValue);

        // Cap healing display at max capacity (overheal shown via badge)
        var healingDisplay = Math.Min(healing, MaxValue - valueAfterDamage);

        BasePercentage = (double)valueAfterDamage / MaxValue * 100;
        DamagePercentage = (double)damage / MaxValue * 100;
        HealingPercentage = (double)Math.Max(0, healingDisplay) / MaxValue * 100;

        Console.WriteLine($"[PendingPoolBar] Calculated: Base={BasePercentage:F1}%, Damage={DamagePercentage:F1}%, Healing={HealingPercentage:F1}%");

        // Calculate color class based on effective health percentage
        BarColorClass = GetBarColorClass(effectiveValue);

        var futureValue = CurrentValue - PendingDamage + PendingHealing;
        TooltipText = $"Current: {CurrentValue}/{MaxValue} | Pending: -{PendingDamage} +{PendingHealing} | Future: {futureValue}/{MaxValue}";
    }

    /// <summary>
    /// Determines the CSS class for the health bar color based on the effective value percentage.
    /// Returns "health-full" (>50%), "health-mid" (25-50%), or "health-low" (<25%).
    /// </summary>
    private string GetBarColorClass(int effectiveValue)
    {
        if (MaxValue <= 0)
            return "health-full";

        // Cap effective value at MaxValue for color calculation (overheal doesn't make it "more green")
        var cappedValue = Math.Min(effectiveValue, MaxValue);
        var percentage = (double)cappedValue / MaxValue * 100;

        return percentage switch
        {
            > 50 => "health-full",
            > 25 => "health-mid",
            _ => "health-low"
        };
    }
}
