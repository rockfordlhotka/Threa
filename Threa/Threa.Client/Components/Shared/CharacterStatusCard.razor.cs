using Microsoft.AspNetCore.Components;
using GameMechanics.GamePlay;

namespace Threa.Client.Components.Shared;

public class CharacterStatusCardBase : ComponentBase
{
    [Parameter] public TableCharacterInfo Character { get; set; } = null!;
    [Parameter] public EventCallback<TableCharacterInfo> OnClick { get; set; }
    [Parameter] public bool IsSelected { get; set; }

    protected string CardBorderClass => GetHealthStateClass();

    /// <summary>
    /// Determines card border color based on health state per CONTEXT.md:
    /// - Green: Healthy (no critical conditions)
    /// - Yellow: Wounded (FAT <= 25% OR VIT <= 50% OR has wounds)
    /// - Red: Critical (VIT <= 25%)
    /// - Dark: Unconscious (FAT <= 0)
    /// </summary>
    protected string GetHealthStateClass()
    {
        if (Character.FatValue <= 0) return "border-dark";

        var vitPercent = Character.VitMax > 0 ? Character.VitValue * 100 / Character.VitMax : 0;
        var fatPercent = Character.FatMax > 0 ? Character.FatValue * 100 / Character.FatMax : 0;

        if (vitPercent <= 25) return "border-danger";
        if (fatPercent <= 25 || vitPercent <= 50 || Character.WoundCount > 0) return "border-warning";
        return "border-success";
    }

    protected string GetConnectionClass()
    {
        return Character.ConnectionStatus switch
        {
            Threa.Dal.Dto.ConnectionStatus.Connected => "connected",
            Threa.Dal.Dto.ConnectionStatus.Disconnected => "disconnected",
            Threa.Dal.Dto.ConnectionStatus.Away => "away",
            _ => ""
        };
    }

    protected string WoundTooltipHtml => string.IsNullOrEmpty(Character.WoundSummary)
        ? "No wounds"
        : Character.WoundSummary.Replace(", ", "<br/>");

    protected string EffectTooltipHtml => string.IsNullOrEmpty(Character.EffectSummary)
        ? "No active effects"
        : Character.EffectSummary.Replace(", ", "<br/>");
}
