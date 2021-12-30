using Csla;
using Csla.Rules;
using GameMechanics.Reference;
using System;
using Threa.Dal;

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
      if (PendingDamage > 0 || PendingHealing > 0)
      {
        // heal
        int heal = PendingHealing / 2;
        PendingHealing -= heal;
        Value += heal;
        // take damage
        int damage = PendingDamage / 2;
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
    private void Fetch(ICharacter existing)
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
    private void Update(ICharacter existing)
    {
      existing.VitValue = Value;
      existing.VitBaseValue = BaseValue;
      existing.VitPendingDamage = PendingDamage;
      existing.VitPendingHealing = PendingHealing;
    }
  }
}
