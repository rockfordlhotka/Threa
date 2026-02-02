using System.Collections.Generic;

namespace Threa.Dal.Dto;

public class Character
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TrueName { get; set; } = string.Empty;
    public string Aliases { get; set; } = string.Empty;
    public bool IsPlayable { get; set; }
    public bool IsNpc { get; set; }
    public bool IsTemplate { get; set; }
    public bool VisibleToPlayers { get; set; } = true;
    public string Species { get; set; } = string.Empty;
    public int DamageClass { get; set; }
    public string Height { get; set; } = string.Empty;
    public string Weight { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string SkinDescription { get; set; } = string.Empty;
    public string HairDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long Birthdate { get; set; }
    public int XPTotal { get; set; }
    public int XPBanked { get; set; }
    public int ActionPointMax { get; set; }
    public int ActionPointRecovery { get; set; }
    public int ActionPointAvailable { get; set; }
    public int VitValue { get; set; }
    public int VitBaseValue { get; set; }
    public int VitPendingHealing { get; set; }
    public int VitPendingDamage { get; set; }
    public int FatValue { get; set; }
    public int FatBaseValue { get; set; }
    public int FatPendingHealing { get; set; }
    public int FatPendingDamage { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public List<CharacterAttribute> AttributeList { get; set; } = [];
    public List<CharacterSkill> Skills { get; set; } = [];

    /// <summary>
    /// Items owned by this character.
    /// Note: Items are managed separately via ICharacterItemDal for efficiency.
    /// This list is populated on-demand and may not always be filled.
    /// </summary>
    public List<CharacterItem> Items { get; set; } = [];

    /// <summary>
    /// Active effects on this character (buffs, debuffs, conditions, etc.).
    /// Note: Effects are managed separately via IEffectDal for efficiency.
    /// This list is populated on-demand and may not always be filled.
    /// </summary>
    public List<CharacterEffect> Effects { get; set; } = [];

    /// <summary>
    /// Currency held by this character (in copper pieces for easy calculation).
    /// </summary>
    public int CopperCoins { get; set; }
    public int SilverCoins { get; set; }
    public int GoldCoins { get; set; }
    public int PlatinumCoins { get; set; }

    /// <summary>
    /// Gets the total currency value in copper pieces.
    /// 1 sp = 20 cp, 1 gp = 400 cp, 1 pp = 8000 cp
    /// </summary>
    public int TotalCopperValue => CopperCoins + (SilverCoins * 20) + (GoldCoins * 400) + (PlatinumCoins * 8000);

    /// <summary>
    /// Current game time in seconds from epoch 0.
    /// Used for epoch-based effect expiration.
    /// </summary>
    public long CurrentGameTimeSeconds { get; set; }

    // Template organization properties (for NPC templates)

    /// <summary>
    /// Folder-like grouping for NPC templates (e.g., "Humanoids", "Beasts", "Undead").
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Comma-separated tags for filtering NPC templates (e.g., "minion,melee", "boss,caster").
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// GM notes about template usage (tactics, encounter suggestions, etc.).
    /// </summary>
    public string? TemplateNotes { get; set; }

    /// <summary>
    /// Default attitude when NPC is instantiated from template.
    /// </summary>
    public NpcDisposition DefaultDisposition { get; set; } = NpcDisposition.Hostile;

    /// <summary>
    /// Auto-calculated threat level based on combat AS values.
    /// Higher values indicate more dangerous NPCs.
    /// </summary>
    public int DifficultyRating { get; set; }
}
