using System.Collections.Generic;

namespace Threa.Dal.Dto;

public class Character
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public string Name { get; set; }
    public string Aliases { get; set; }
    public string Species { get; set; }
    public int DamageClass { get; set; }
    public string Height { get; set; }
    public string Weight { get; set; }
    public string Notes { get; set; }
    public string SkinDescription { get; set; }
    public string HairDescription { get; set; }
    public string Description { get; set; }
    public long Birthdate { get; set; }
    public double XPTotal { get; set; }
    public double XPBanked { get; set; }
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
    public string ImageUrl { get; set; }
    public List<CharacterAttribute> AttributeList { get; set; } = [];
    public List<CharacterSkill> Skills { get; set; } = [];
    public List<Wound> Wounds { get; set; } = [];

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
}
