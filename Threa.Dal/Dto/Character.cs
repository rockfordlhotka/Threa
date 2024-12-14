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
}
