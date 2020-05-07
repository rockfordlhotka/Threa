using Csla;
using Csla.Rules;
using GameMechanics.Reference;
using System;

namespace GameMechanics
{
  [Serializable]
  public class Fatigue : BusinessBase<Fatigue>
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
        // recover fatigue
        var vit = Character.Vitality.Value;
        if (vit > 7)
          PendingDamage += 1;
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
          Character.Vitality.PendingDamage += overflow * 2;
        }
        CheckFocusRolls();
      }
    }

    private void CheckFocusRolls()
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

    [CreateChild]

    private void Create(Character character)
    {
      Value = BaseValue = Calculate.GetValue(character.Endurance.Value, character.Willpower.Value);
    }

    private class Calculate : BusinessRule
    {
#pragma warning disable CSLA0017 // Find Business Rules That Do Not Use Add() Methods on the Context
      protected override void Execute(IRuleContext context)
#pragma warning restore CSLA0017 // Find Business Rules That Do Not Use Add() Methods on the Context
      {
        var target = (Fatigue)context.Target;
        var character = target.Character;
        target.BaseValue = GetValue(character.Endurance.Value, character.Willpower.Value);
      }

      public static int GetValue(int endurance, int willpower)
      {
        return ((endurance + willpower) / 2) + 14;
      }
    }

    internal void TakeDamage(DamageValue damageValue)
    {
      PendingDamage += damageValue.GetModifiedDamage(Character.DamageClass);
    }
  }
}
