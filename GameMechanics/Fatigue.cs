using Csla;
using Csla.Rules;
using GameMechanics.Reference;
using System;
using Threa.Dal;

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

    private CharacterEdit Character
    {
      get => (CharacterEdit)Parent;
    }

    public void EndOfRound()
    {
      if (PendingDamage > 0 || PendingHealing > 0)
      {
        // recover fatigue
        var vit = Character.Vitality.Value;
        if (vit > 7)
          PendingHealing += 1;
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
          Character.Vitality.PendingDamage += overflow;
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

    internal void TakeDamage(DamageValue damageValue)
    {
      PendingDamage += damageValue.GetModifiedDamage(Character.DamageClass);
    }

    [CreateChild]

    private void Create(CharacterEdit character)
    {
      var end = character.GetAttribute("END");
      var wil = character.GetAttribute("WIL");
      Value = BaseValue = (end + wil) / 2 - 5;
    }

    [FetchChild]
    private void Fetch(ICharacter existing)
    {
      using (BypassPropertyChecks)
      {
        Value = existing.FatValue;
        BaseValue = existing.FatBaseValue;
        PendingDamage = existing.FatPendingDamage;
        PendingHealing = existing.FatPendingHealing;
      }
    }

    [UpdateChild]
    [InsertChild]
    private void Update(ICharacter existing)
    {
      existing.FatValue = Value;
      existing.FatBaseValue = BaseValue;
      existing.FatPendingDamage = PendingDamage;
      existing.FatPendingHealing = PendingHealing;
    }
  }
}
