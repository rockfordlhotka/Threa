using Csla;
using GameMechanics.Effects;
using GameMechanics.Reference;
using System;
using System.Linq;
using Threa.Dal.Dto;

namespace GameMechanics
{
  [Serializable]
  public class Damage : BusinessBase<Damage>
  {
    public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(nameof(Name));
    public string Name
    {
      get => GetProperty(NameProperty);
      private set => LoadProperty(NameProperty, value);
    }

    public static readonly PropertyInfo<int> ValueProperty = RegisterProperty<int>(nameof(Value));
    public int Value
    {
      get => GetProperty(ValueProperty);
      private set => LoadProperty(ValueProperty, value);
    }

    public static readonly PropertyInfo<int> BaseValueProperty = RegisterProperty<int>(nameof(BaseValue));
    public int BaseValue
    {
      get => GetProperty(BaseValueProperty);
      private set => LoadProperty(BaseValueProperty, value);
    }

    public static readonly PropertyInfo<int> PendingHealingProperty = RegisterProperty<int>(nameof(PendingHealing));
    public int PendingHealing
    {
      get => GetProperty(PendingHealingProperty);
      set => SetProperty(PendingHealingProperty, value);
    }

    public static readonly PropertyInfo<int> PendingDamageProperty = RegisterProperty<int>(nameof(PendingDamage));
    public int PendingDamage
    {
      get => GetProperty(PendingDamageProperty);
      set => SetProperty(PendingDamageProperty, value);
    }

    private CharacterEdit Character
    {
      get => (CharacterEdit)((DamageList)Parent).Parent;
    }

    public void EndOfRound(IChildDataPortal<EffectRecord>? effectPortal = null)
    {
      if (Name == "FAT")
        EndOfRoundFat();
      else
        EndOfRoundVit(effectPortal);
    }

    private void EndOfRoundFat()
    {
      if (Value < BaseValue || PendingDamage > 0)
      {
        // recover fatigue
        var vit = Character.Vitality.Value;
        if (vit > 7)
          PendingHealing += 1;
        // heal
        int heal = PendingHealing / 2;
        if (PendingHealing == 1)
          heal = 1;
        PendingHealing -= heal;
        Value += heal;
        // take damage
        int damage = PendingDamage / 2;
        if (PendingDamage == 1)
          damage = 1;
        PendingDamage -= damage;
        Value -= damage;
        // cascade overflow
        if (Value < 0)
        {
          var overflow = -Value;
          Value = 0;
          Character.Vitality.PendingDamage += overflow * 2;
        }
        CheckFatFocusRolls();
      }
    }

    private void EndOfRoundVit(IChildDataPortal<EffectRecord>? effectPortal = null)
    {
      if (Value < BaseValue || PendingDamage > 0)
      {
        // heal
        int heal = PendingHealing / 2;
        if (PendingHealing == 1)
          heal = 1;
        PendingHealing -= heal;
        Value += heal;
        // take damage
        int damage = PendingDamage / 2;
        if (PendingDamage == 1)
          damage = 1;
        PendingDamage -= damage;
        Value -= damage;
        // cascade overflow
        if (Value < 0)
        {
          var overflow = -Value;
          Value = 0;
          if (effectPortal != null)
          {
            for (int i = 0; i < overflow; i++)
            {
              var location = EffectList.GetRandomLocation();
              Effects.Behaviors.WoundBehavior.TakeWound(Character, location, effectPortal);
            }
          }
        }
        CheckVitFocusRolls();
      }
    }

    private void CheckFatFocusRolls()
    {
      var passedOut = false;
      if (Value < 1)
        passedOut = true;
      else if (Value < 2 && Character.Skills.SkillCheck("Focus", 12).Success)
        passedOut = true;
      else if (Value < 4 && Character.Skills.SkillCheck("Focus", 7).Success)
        passedOut = true;
      else if (Value < 6 && Character.Skills.SkillCheck("Focus", 5).Success)
        passedOut = true;

      if (passedOut)
        Character.IsPassedOut = true;
    }

    private void CheckVitFocusRolls()
    {
      var passedOut = false;
      if (Value < 2)
        passedOut = true;
      else if (Value < 4 && Character.Skills.SkillCheck("Focus", 12).Success)
        passedOut = true;
      else if (Value < 6 && Character.Skills.SkillCheck("Focus", 7).Success)
        passedOut = true;

      if (passedOut)
        Character.IsPassedOut = true;
    }

    public void TakeDamage(DamageValue damageValue)
    {
      if (Name == "FAT")
        TakeFatDamage(damageValue);
      else
        TakeVitDamage(damageValue);
    }

    private void TakeFatDamage(DamageValue damageValue)
    {
      PendingDamage += damageValue.GetModifiedDamage(Character.DamageClass);
    }

    private void TakeVitDamage(DamageValue damageValue)
    {
      var dmg = damageValue.GetModifiedDamage(Character.DamageClass);
      if (dmg == 5)
        PendingDamage += 1;
      else if (dmg == 6)
        PendingDamage += 2;
      else if (dmg == 7)
        PendingDamage += 4;
      else if (dmg == 8)
        PendingDamage += 6;
      else if (dmg == 9)
        PendingDamage += 8;
      else if (dmg > 9)
        PendingDamage += dmg;
    }

    [CreateChild]
    private void Create(string name, CharacterEdit character)
    {
      Name = name;
      int start;
      switch (name)
      {
        case "FAT":
          var end = character.AttributeList.Where(r => r.Name == "END").First().BaseValue;
          var wil = character.AttributeList.Where(r => r.Name == "WIL").First().BaseValue;
          start = end + wil - 5;
          break;
        case "VIT":
          var str = character.AttributeList.Where(r => r.Name == "STR").First().BaseValue;
          start = str * 2 - 5;
          break;
        default:
          throw new InvalidOperationException(name);
      }
      Value = BaseValue = start;
    }

    [FetchChild]
    private void Fetch(string name, Character character)
    {
      using (BypassPropertyChecks)
      {
        Name = name;
        switch (name)
        {
          case "FAT":
            BaseValue = character.FatBaseValue;
            Value = character.FatValue;
            PendingDamage = character.FatPendingDamage;
            PendingHealing = character.FatPendingHealing;
            break;
          case "VIT":
            BaseValue = character.VitBaseValue;
            Value = character.VitValue;
            PendingDamage = character.VitPendingDamage;
            PendingHealing = character.VitPendingHealing;
            break;
          default:
            throw new InvalidOperationException(name);
        }
      }
    }
  }
}
