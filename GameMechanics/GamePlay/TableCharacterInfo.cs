using Csla;
using System;
using System.Linq;
using Threa.Dal.Dto;

namespace GameMechanics.GamePlay;

/// <summary>
/// Read-only information about a character connected to a table.
/// </summary>
[Serializable]
public class TableCharacterInfo : ReadOnlyBase<TableCharacterInfo>
{
    public static readonly PropertyInfo<Guid> TableIdProperty = RegisterProperty<Guid>(nameof(TableId));
    public Guid TableId
    {
        get => GetProperty(TableIdProperty);
        private set => LoadProperty(TableIdProperty, value);
    }

    public static readonly PropertyInfo<int> CharacterIdProperty = RegisterProperty<int>(nameof(CharacterId));
    public int CharacterId
    {
        get => GetProperty(CharacterIdProperty);
        private set => LoadProperty(CharacterIdProperty, value);
    }

    public static readonly PropertyInfo<int> PlayerIdProperty = RegisterProperty<int>(nameof(PlayerId));
    public int PlayerId
    {
        get => GetProperty(PlayerIdProperty);
        private set => LoadProperty(PlayerIdProperty, value);
    }

    public static readonly PropertyInfo<DateTime> JoinedAtProperty = RegisterProperty<DateTime>(nameof(JoinedAt));
    public DateTime JoinedAt
    {
        get => GetProperty(JoinedAtProperty);
        private set => LoadProperty(JoinedAtProperty, value);
    }

    public static readonly PropertyInfo<ConnectionStatus> ConnectionStatusProperty = RegisterProperty<ConnectionStatus>(nameof(ConnectionStatus));
    public ConnectionStatus ConnectionStatus
    {
        get => GetProperty(ConnectionStatusProperty);
        private set => LoadProperty(ConnectionStatusProperty, value);
    }

    public static readonly PropertyInfo<DateTime?> LastActivityProperty = RegisterProperty<DateTime?>(nameof(LastActivity));
    public DateTime? LastActivity
    {
        get => GetProperty(LastActivityProperty);
        private set => LoadProperty(LastActivityProperty, value);
    }

    // Character details (loaded separately)
    public static readonly PropertyInfo<string> CharacterNameProperty = RegisterProperty<string>(nameof(CharacterName));
    public string CharacterName
    {
        get => GetProperty(CharacterNameProperty);
        private set => LoadProperty(CharacterNameProperty, value);
    }

    public static readonly PropertyInfo<string> SpeciesProperty = RegisterProperty<string>(nameof(Species));
    public string Species
    {
        get => GetProperty(SpeciesProperty);
        private set => LoadProperty(SpeciesProperty, value);
    }

    public static readonly PropertyInfo<int> FatValueProperty = RegisterProperty<int>(nameof(FatValue));
    public int FatValue
    {
        get => GetProperty(FatValueProperty);
        private set => LoadProperty(FatValueProperty, value);
    }

    public static readonly PropertyInfo<int> FatMaxProperty = RegisterProperty<int>(nameof(FatMax));
    public int FatMax
    {
        get => GetProperty(FatMaxProperty);
        private set => LoadProperty(FatMaxProperty, value);
    }

    public static readonly PropertyInfo<int> VitValueProperty = RegisterProperty<int>(nameof(VitValue));
    public int VitValue
    {
        get => GetProperty(VitValueProperty);
        private set => LoadProperty(VitValueProperty, value);
    }

    public static readonly PropertyInfo<int> VitMaxProperty = RegisterProperty<int>(nameof(VitMax));
    public int VitMax
    {
        get => GetProperty(VitMaxProperty);
        private set => LoadProperty(VitMaxProperty, value);
    }

    public static readonly PropertyInfo<int> ActionPointsProperty = RegisterProperty<int>(nameof(ActionPoints));
    public int ActionPoints
    {
        get => GetProperty(ActionPointsProperty);
        private set => LoadProperty(ActionPointsProperty, value);
    }

    public static readonly PropertyInfo<int> ActionPointMaxProperty = RegisterProperty<int>(nameof(ActionPointMax));
    public int ActionPointMax
    {
        get => GetProperty(ActionPointMaxProperty);
        private set => LoadProperty(ActionPointMaxProperty, value);
    }

    // Pending pool properties
    public static readonly PropertyInfo<int> FatPendingDamageProperty = RegisterProperty<int>(nameof(FatPendingDamage));
    public int FatPendingDamage
    {
        get => GetProperty(FatPendingDamageProperty);
        private set => LoadProperty(FatPendingDamageProperty, value);
    }

    public static readonly PropertyInfo<int> FatPendingHealingProperty = RegisterProperty<int>(nameof(FatPendingHealing));
    public int FatPendingHealing
    {
        get => GetProperty(FatPendingHealingProperty);
        private set => LoadProperty(FatPendingHealingProperty, value);
    }

    public static readonly PropertyInfo<int> VitPendingDamageProperty = RegisterProperty<int>(nameof(VitPendingDamage));
    public int VitPendingDamage
    {
        get => GetProperty(VitPendingDamageProperty);
        private set => LoadProperty(VitPendingDamageProperty, value);
    }

    public static readonly PropertyInfo<int> VitPendingHealingProperty = RegisterProperty<int>(nameof(VitPendingHealing));
    public int VitPendingHealing
    {
        get => GetProperty(VitPendingHealingProperty);
        private set => LoadProperty(VitPendingHealingProperty, value);
    }

    // Status count properties
    public static readonly PropertyInfo<int> WoundCountProperty = RegisterProperty<int>(nameof(WoundCount));
    public int WoundCount
    {
        get => GetProperty(WoundCountProperty);
        private set => LoadProperty(WoundCountProperty, value);
    }

    public static readonly PropertyInfo<int> EffectCountProperty = RegisterProperty<int>(nameof(EffectCount));
    public int EffectCount
    {
        get => GetProperty(EffectCountProperty);
        private set => LoadProperty(EffectCountProperty, value);
    }

    // Tooltip summary string properties
    public static readonly PropertyInfo<string> WoundSummaryProperty = RegisterProperty<string>(nameof(WoundSummary));
    public string WoundSummary
    {
        get => GetProperty(WoundSummaryProperty);
        private set => LoadProperty(WoundSummaryProperty, value);
    }

    public static readonly PropertyInfo<string> EffectSummaryProperty = RegisterProperty<string>(nameof(EffectSummary));
    public string EffectSummary
    {
        get => GetProperty(EffectSummaryProperty);
        private set => LoadProperty(EffectSummaryProperty, value);
    }

    public static readonly PropertyInfo<string?> GmNotesProperty = RegisterProperty<string?>(nameof(GmNotes));
    /// <summary>
    /// GM-only notes for this character at this table. Not visible to players.
    /// </summary>
    public string? GmNotes
    {
        get => GetProperty(GmNotesProperty);
        private set => LoadProperty(GmNotesProperty, value);
    }

    // Concentration status
    public static readonly PropertyInfo<bool> IsConcentratingProperty = RegisterProperty<bool>(nameof(IsConcentrating));
    /// <summary>
    /// Whether the character is currently concentrating on an effect.
    /// </summary>
    public bool IsConcentrating
    {
        get => GetProperty(IsConcentratingProperty);
        private set => LoadProperty(IsConcentratingProperty, value);
    }

    public static readonly PropertyInfo<string?> ConcentrationNameProperty = RegisterProperty<string?>(nameof(ConcentrationName));
    /// <summary>
    /// Name of the effect being concentrated on (spell name or effect name).
    /// </summary>
    public string? ConcentrationName
    {
        get => GetProperty(ConcentrationNameProperty);
        private set => LoadProperty(ConcentrationNameProperty, value);
    }

    // NPC-specific properties for dashboard display
    public static readonly PropertyInfo<bool> IsNpcProperty = RegisterProperty<bool>(nameof(IsNpc));
    /// <summary>
    /// Whether this character is an NPC (vs a player character).
    /// </summary>
    public bool IsNpc
    {
        get => GetProperty(IsNpcProperty);
        private set => LoadProperty(IsNpcProperty, value);
    }

    public static readonly PropertyInfo<NpcDisposition> DispositionProperty = RegisterProperty<NpcDisposition>(nameof(Disposition));
    /// <summary>
    /// The NPC's disposition toward players (Hostile, Neutral, Friendly).
    /// </summary>
    public NpcDisposition Disposition
    {
        get => GetProperty(DispositionProperty);
        private set => LoadProperty(DispositionProperty, value);
    }

    public static readonly PropertyInfo<int?> SourceTemplateIdProperty = RegisterProperty<int?>(nameof(SourceTemplateId));
    /// <summary>
    /// ID of the template this NPC was spawned from (null for PCs/templates).
    /// </summary>
    public int? SourceTemplateId
    {
        get => GetProperty(SourceTemplateIdProperty);
        private set => LoadProperty(SourceTemplateIdProperty, value);
    }

    public static readonly PropertyInfo<string?> SourceTemplateNameProperty = RegisterProperty<string?>(nameof(SourceTemplateName));
    /// <summary>
    /// Name of the source template for display (e.g., "From: Goblin Warrior").
    /// </summary>
    public string? SourceTemplateName
    {
        get => GetProperty(SourceTemplateNameProperty);
        private set => LoadProperty(SourceTemplateNameProperty, value);
    }

    public string ConnectionStatusDisplay => ConnectionStatus switch
    {
        ConnectionStatus.Connected => "Connected",
        ConnectionStatus.Disconnected => "Disconnected",
        ConnectionStatus.Away => "Away",
        _ => "Unknown"
    };

    [FetchChild]
    private void Fetch(TableCharacter tableChar, Character? character)
    {
        TableId = tableChar.TableId;
        CharacterId = tableChar.CharacterId;
        PlayerId = tableChar.PlayerId;
        JoinedAt = tableChar.JoinedAt;
        ConnectionStatus = tableChar.ConnectionStatus;
        LastActivity = tableChar.LastActivity;

        if (character != null)
        {
            CharacterName = character.Name;
            Species = character.Species;
            FatValue = character.FatValue;
            FatMax = character.FatBaseValue;
            VitValue = character.VitValue;
            VitMax = character.VitBaseValue;
            ActionPoints = character.ActionPointAvailable;
            ActionPointMax = character.ActionPointMax;

            // Load pending pool values
            FatPendingDamage = character.FatPendingDamage;
            FatPendingHealing = character.FatPendingHealing;
            VitPendingDamage = character.VitPendingDamage;
            VitPendingHealing = character.VitPendingHealing;

            // Calculate wound and effect counts from Effects list
            var wounds = character.Effects?.Where(e => e.EffectType == EffectType.Wound).ToList() ?? [];
            var nonWoundEffects = character.Effects?.Where(e => e.EffectType != EffectType.Wound).ToList() ?? [];

            WoundCount = wounds.Count;
            EffectCount = nonWoundEffects.Count;

            // Build wound summary (e.g., "Light x2, Serious x1")
            var woundGroups = wounds
                .GroupBy(e => e.Name)
                .Select(g => g.Count() > 1 ? $"{g.Key} x{g.Count()}" : g.Key);
            WoundSummary = string.Join(", ", woundGroups);

            // Build effect summary (e.g., "Poison (3 rnd), Blessed, Shield Spell")
            var effectDescriptions = nonWoundEffects
                .Select(e => e.RoundsRemaining.HasValue ? $"{e.Name} ({e.RoundsRemaining} rnd)" : e.Name);
            EffectSummary = string.Join(", ", effectDescriptions);

            // Check concentration status
            var concentrationEffect = character.Effects?.FirstOrDefault(e => e.EffectType == EffectType.Concentration);
            IsConcentrating = concentrationEffect != null;
            if (concentrationEffect != null)
            {
                // Try to get the spell name from custom properties (behavior state stored there)
                var stateJson = concentrationEffect.CustomProperties;
                if (!string.IsNullOrEmpty(stateJson))
                {
                    try
                    {
                        var state = System.Text.Json.JsonSerializer.Deserialize<ConcentrationStateMinimal>(stateJson);
                        ConcentrationName = state?.SpellName ?? concentrationEffect.Name;
                    }
                    catch
                    {
                        ConcentrationName = concentrationEffect.Name;
                    }
                }
                else
                {
                    ConcentrationName = concentrationEffect.Name;
                }
            }

            // NPC-specific properties
            IsNpc = character.IsNpc;
            Disposition = character.DefaultDisposition;
            SourceTemplateId = character.SourceTemplateId;
            SourceTemplateName = character.SourceTemplateName;
        }

        // Load GM notes from table character record
        GmNotes = tableChar.GmNotes;
    }
}

/// <summary>
/// Minimal DTO for deserializing concentration state to get spell name.
/// </summary>
file class ConcentrationStateMinimal
{
    [System.Text.Json.Serialization.JsonPropertyName("spellName")]
    public string? SpellName { get; set; }
}
