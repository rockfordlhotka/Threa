using Csla;
using Csla.Rules;
using GameMechanics.Reference;
using System;

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

    private Character Character
    {
      get => (Character)Parent;
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

    [CreateChild]

    private void Create(Character character)
    {
      Value = BaseValue = Calculate.GetValue(character.Strength.Value);
    }

    private class Calculate : BusinessRule
    {
#pragma warning disable CSLA0017 // Find Business Rules That Do Not Use Add() Methods on the Context
      protected override void Execute(IRuleContext context)
#pragma warning restore CSLA0017 // Find Business Rules That Do Not Use Add() Methods on the Context
      {
        var target = (Vitality)context.Target;
        var character = target.Character;
        target.BaseValue = GetValue(character.Strength.Value);
      }

      public static int GetValue(int strength)
      {
        return strength + 14;
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
  }
}
