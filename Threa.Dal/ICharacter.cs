using System;
using System.Collections.Generic;
using System.Reflection;

namespace Threa.Dal
{
  public interface ICharacter
  {
    int Id { get; set; }
    int PlayerId { get; set; }
    string Name { get; set; }
    string TrueName { get; set; }
    string Aliases { get; set; }
    string Species { get; set; }
    int DamageClass { get; set; }
    string Height { get; set; }
    string Weight { get; set; }
    string Notes { get; set; }
    string SkinDescription { get; set; }
    string HairDescription { get; set; }
    string Description { get; set; }
    long Birthdate { get; set; }
    bool IsPassedOut { get; set; }
    double XPTotal { get; set; }
    double XPBanked { get; set; }
    bool IsPlayable { get; set; }
    int ActionPointMax { get; set; }
    int ActionPointRecovery { get; set; }
    int ActionPointAvailable { get; set; }
    int VitValue { get; set; }
    int VitBaseValue { get; set; }
    int VitPendingHealing { get; set; }
    int VitPendingDamage { get; set; }
    int FatValue { get; set; }
    int FatBaseValue { get; set; }
    int FatPendingHealing { get; set; }
    int FatPendingDamage { get; set; }
    string ImageUrl { get; set; }
    List<ICharacterAttribute> AttributeList { get; set; }
    List<ICharacterSkill> Skills { get; set; }
    List<IWound> Wounds { get; set; }
  }
}
