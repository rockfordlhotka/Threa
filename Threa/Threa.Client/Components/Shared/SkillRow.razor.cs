using Microsoft.AspNetCore.Components;

namespace Threa.Client.Components.Shared;

public class SkillRowBase : ComponentBase
{
    [Parameter] public string SkillName { get; set; } = "";
    [Parameter] public int Level { get; set; }
    [Parameter] public int AbilityScore { get; set; }
    [Parameter] public string PrimaryAttribute { get; set; } = "";
    [Parameter] public EventCallback OnUse { get; set; }
    [Parameter] public bool CanUse { get; set; } = true;

    protected string TooltipText => $"{SkillName} (Level {Level}) - AS: {AbilityScore} [{PrimaryAttribute}]";
}
