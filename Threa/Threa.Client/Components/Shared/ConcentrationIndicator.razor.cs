using Microsoft.AspNetCore.Components;
using GameMechanics;
using GameMechanics.Effects.Behaviors;
using Threa.Dal.Dto;

namespace Threa.Client.Components.Shared;

/// <summary>
/// Displays concentration status for a character.
/// Supports both casting-time (progress bar) and sustained (drain display) concentration types.
/// </summary>
public partial class ConcentrationIndicator : ComponentBase
{
    /// <summary>
    /// The character to display concentration status for.
    /// </summary>
    [Parameter]
    public CharacterEdit? Character { get; set; }

    /// <summary>
    /// If true, shows compact view (badge only, no progress/details).
    /// Used in CharacterStatusCard for GM dashboard.
    /// </summary>
    [Parameter]
    public bool Compact { get; set; }

    /// <summary>
    /// If true, shows the Drop Concentration button.
    /// </summary>
    [Parameter]
    public bool ShowDropButton { get; set; } = true;

    /// <summary>
    /// Callback when concentration is dropped voluntarily.
    /// </summary>
    [Parameter]
    public EventCallback OnConcentrationDropped { get; set; }

    private bool IsConcentrating => Character != null && ConcentrationBehavior.IsConcentrating(Character);

    private ConcentrationState? State
    {
        get
        {
            if (Character == null) return null;
            var effect = Character.GetConcentrationEffect();
            if (effect == null) return null;
            return ConcentrationState.FromJson(effect.BehaviorState);
        }
    }

    private bool IsCastingTime => State?.ConcentrationType is "MagazineReload" or "SpellCasting" or "RitualPreparation";
    private bool IsSustained => State?.ConcentrationType is "SustainedSpell" or "SustainedAbility" or "MentalControl";

    private int CurrentProgress => State?.CurrentProgress ?? 0;
    private int TotalRequired => State?.TotalRequired ?? 1;
    private int LinkedEffectCount => State?.LinkedEffectIds?.Count ?? 0;

    private double ProgressPercent
    {
        get
        {
            if (TotalRequired <= 0) return 0;
            return Math.Min(100, (double)CurrentProgress / TotalRequired * 100);
        }
    }

    private string? DrainDisplay
    {
        get
        {
            if (State == null) return null;
            var parts = new List<string>();
            if (State.FatDrainPerRound > 0)
                parts.Add($"{State.FatDrainPerRound} FAT");
            if (State.VitDrainPerRound > 0)
                parts.Add($"{State.VitDrainPerRound} VIT");
            return parts.Count > 0 ? string.Join(" + ", parts) : null;
        }
    }

    private string GetConcentrationName()
    {
        if (State?.SpellName != null)
            return State.SpellName;

        var effect = Character?.GetConcentrationEffect();
        if (effect?.Name != null)
            return effect.Name;

        return State?.ConcentrationType ?? "Unknown";
    }

    private async Task OnDropConcentration()
    {
        if (Character != null)
        {
            ConcentrationBehavior.BreakConcentration(Character, "Voluntarily dropped concentration");
            await OnConcentrationDropped.InvokeAsync();
        }
    }
}
