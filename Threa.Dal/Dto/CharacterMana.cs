using System;

namespace Threa.Dal.Dto;

/// <summary>
/// Represents a character's mana pool for a specific magic school.
/// Max mana equals the character's skill level in that school's mana skill.
/// </summary>
public class CharacterMana
{
    /// <summary>
    /// Unique identifier for this mana pool record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The character who owns this mana pool.
    /// </summary>
    public int CharacterId { get; set; }

    /// <summary>
    /// The magic school this mana pool belongs to.
    /// </summary>
    public MagicSchool MagicSchool { get; set; }

    /// <summary>
    /// Current mana available in this pool.
    /// </summary>
    public int CurrentMana { get; set; }

    /// <summary>
    /// The skill ID for the mana skill of this school (e.g., "fire-mana", "life-mana").
    /// The skill's level determines MaxMana.
    /// </summary>
    public string ManaSkillId { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp of last mana change (for tracking recovery timing).
    /// </summary>
    public DateTime LastUpdated { get; set; }
}
