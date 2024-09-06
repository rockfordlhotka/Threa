using Csla;
using GameMechanics.Reference;
using System;
using Threa.Dal.Dto;

namespace GameMechanics
{
  [Serializable]
  public class Vitality : BusinessBase<Vitality>
  {
    public static readonly PropertyInfo<int> ValueProperty = RegisterProperty<int>(nameof(Value));
    public int Value
    {
      get => GetProperty(ValueProperty);
      set => SetProperty(ValueProperty, value);
    }

    public static readonly PropertyInfo<int> BaseValueProperty = RegisterProperty<int>(nameof(BaseValue));
    public int BaseValue
    {
      get => GetProperty(BaseValueProperty);
      private set => LoadProperty(BaseValueProperty, value);
    }

    public static readonly PropertyInfo<int> PendingDamageProperty = RegisterProperty<int>(nameof(PendingDamage));
    public int PendingDamage
    {
      get => GetProperty(PendingDamageProperty);
      set => SetProperty(PendingDamageProperty, value);
    }

    public static readonly PropertyInfo<int> PendingHealingProperty = RegisterProperty<int>(nameof(PendingHealing));
    public int PendingHealing
    {
      get => GetProperty(PendingHealingProperty);
      set => SetProperty(PendingHealingProperty, value);
    }

    private CharacterEdit Character
    {
      get => (CharacterEdit)Parent;
    }

    public void EndOfRound()
    {
      if (Value < BaseValue)
      {
        // recover
        if (Character.Vitality.Value > Character.Vitality.BaseValue / 2)
          PendingHealing += 1;
        // heal
        int heal;
        if (PendingHealing > 1)
          heal = PendingHealing / 2;
        else
          heal = 1;
        Value += heal;
        PendingHealing -= heal;
      }
      else if (PendingHealing > 0)
      {
        if (PendingHealing > 1)
          PendingHealing /= 2;
        else
          PendingHealing = 0;
      }
      if (PendingDamage > 0)
      {
        // take damage
        int damage;
        if (PendingDamage > 2)
          damage = PendingDamage / 2;
        else
          damage = 1;
        PendingDamage -= damage;
        Value -= damage;
        // cascade overflow
        if (Value < 0)
        {
          var overflow = -Value;
          Value = 0;
          Character.Wounds.TakeWound(overflow);
        }
      }
    }

    internal void TakeDamage(DamageValue damageValue)
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
    private void Create(CharacterEdit character)
    {
      Value = BaseValue = character.GetAttribute("STR") * 2 - 5;
    }

    [FetchChild]
    private void Fetch(Character existing)
    {
      using (BypassPropertyChecks)
      {
        Value = existing.VitValue;
        BaseValue = existing.VitBaseValue;
        PendingDamage = existing.VitPendingDamage;
        PendingHealing = existing.VitPendingHealing;
      }
    }

    [UpdateChild]
    [InsertChild]
    private void Update(Character existing)
    {
      existing.VitValue = Value;
      existing.VitBaseValue = BaseValue;
      existing.VitPendingDamage = PendingDamage;
      existing.VitPendingHealing = PendingHealing;
    }
  }
}
