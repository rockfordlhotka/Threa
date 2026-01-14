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
        // First try to get icon by effect name (more specific), then fall back to effect type
        IconClass = GetIconForEffectName(EffectName) ?? GetIconForEffectType(EffectType);
    }

    private string? GetIconForEffectName(string effectName)
    {
        if (string.IsNullOrWhiteSpace(effectName))
            return null;

        return effectName.ToLowerInvariant() switch
        {
            // Conditions
            "stunned" or "stun" => "bi-stars",
            "blinded" or "blind" => "bi-eye-slash-fill",
            "deafened" or "deaf" => "bi-ear-fill",
            "prone" => "bi-person-fill-down",
            "unconscious" => "bi-person-fill-x",
            "paralyzed" or "paralysis" => "bi-lightning-fill",
            "frightened" or "fear" => "bi-emoji-dizzy",
            "charmed" or "charm" => "bi-heart-fill",
            "invisible" or "invisibility" => "bi-eye-slash",
            "grappled" or "grapple" => "bi-hand-index-fill",
            "restrained" => "bi-lock-fill",
            "incapacitated" => "bi-x-circle-fill",
            "exhausted" or "exhaustion" => "bi-battery-half",
            "confused" or "confusion" => "bi-question-circle-fill",
            "silenced" or "silence" => "bi-mic-mute-fill",
            "slowed" or "slow" => "bi-hourglass-split",
            "hasted" or "haste" => "bi-lightning-charge-fill",
            // Damage over time
            "bleeding" or "bleed" => "bi-droplet-half",
            "burning" or "burn" or "on fire" => "bi-fire",
            "poisoned" => "bi-moisture",
            "frozen" or "freeze" => "bi-snow",
            // Other specific effects
            "regenerating" or "regeneration" => "bi-heart-pulse-fill",
            "shielded" or "shield" => "bi-shield-fill",
            "blessed" or "blessing" => "bi-star-fill",
            "cursed" or "curse" => "bi-emoji-angry-fill",
            _ => null  // Fall back to type-based icon
        };
    }

    private string GetIconForEffectType(string effectType)
    {
        return effectType.ToLowerInvariant() switch
        {
            "wound" => "bi-bandaid",
            "condition" => "bi-exclamation-triangle-fill",
            "poison" => "bi-moisture",
            "disease" => "bi-virus",
            "buff" => "bi-arrow-up-circle-fill",
            "debuff" => "bi-arrow-down-circle-fill",
            "spelleffect" => "bi-magic",
            "objecteffect" => "bi-box-fill",
            "environmental" => "bi-cloud-fill",
            "itemeffect" => "bi-gem",
            "combatstance" => "bi-shield-shaded",
            // Legacy mappings
            "stun" => "bi-stars",
            "invisibility" => "bi-eye-slash",
            "bleed" => "bi-droplet-half",
            _ => "bi-question-diamond",
        };
    }
}
