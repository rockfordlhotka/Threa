using Microsoft.AspNetCore.Components;

namespace Threa.Client.Components.Shared;

public class EffectIconBase : ComponentBase
{
    [Parameter] public string EffectType { get; set; } = "";
    [Parameter] public string EffectName { get; set; } = "";
    [Parameter] public string Tooltip { get; set; } = "";
    [Parameter] public int? Stacks { get; set; }
    [Parameter] public string Color { get; set; } = "black";

    protected string IconClass { get; private set; } = "bi-question-diamond";

    protected override void OnParametersSet()
    {
        IconClass = GetIconForEffectType(EffectType);
    }

    private string GetIconForEffectType(string effectType)
    {
        return effectType.ToLowerInvariant() switch
        {
            "poison" => "bi-moisture",
            "stun" => "bi-stars",
            "invisibility" => "bi-eye-slash",
            "bleed" => "bi-droplet-half",
            "buff" => "bi-arrow-up-circle",
            "debuff" => "bi-arrow-down-circle",
            "wound" => "bi-bandaid",
            _ => "bi-question-diamond",
        };
    }
}
