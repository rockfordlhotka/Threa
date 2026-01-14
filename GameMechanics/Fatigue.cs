using Csla;
using GameMechanics.Reference;
using System;
using Threa.Dal.Dto;

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
      // Passive FAT recovery depends on current VIT level:
      // VIT >= 5: 1 per round (here)
      // VIT = 4: 1 per minute (handled in TimeManager)
      // VIT = 3: 1 per 30 minutes (handled in TimeManager)
      // VIT = 2: 1 per hour (handled in TimeManager)
      // VIT <= 1: No recovery
      if (Value < BaseValue && Character.Vitality.Value >= 5)
      {
        PendingHealing += 1;
      }

      // Apply half of pending healing (rounded up to ensure pool reaches zero)
      if (PendingHealing > 0)
      {
        int heal = (PendingHealing + 1) / 2; // Round up: 1->1, 2->1, 3->2, 4->2, 5->3
        PendingHealing -= heal;
        // Only apply healing up to max
        Value = Math.Min(BaseValue, Value + heal);
      }

      // Apply half of pending damage (rounded up to ensure pool reaches zero)
      if (PendingDamage > 0)
      {
        int damage = (PendingDamage + 1) / 2; // Round up: 1->1, 2->1, 3->2, 4->2, 5->3
        PendingDamage -= damage;
        Value -= damage;
        // cascade overflow to VIT
        if (Value < 0)
        {
          var overflow = -Value;
          Value = 0;
          Character.Vitality.PendingDamage += overflow;
        }
      }

      CheckFocusRolls();
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

    internal void CalculateBase(CharacterEdit character)
    {
      var end = character.GetAttribute("END");
      var wil = character.GetAttribute("WIL");
      BaseValue = end + wil - 5;
    }

    [CreateChild]

    private void Create(CharacterEdit character)
    {
      CalculateBase(character);
      Value = BaseValue;
    }

    [FetchChild]
    private void Fetch(Character existing)
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
    private void Update(Character existing)
    {
      existing.FatValue = Value;
      existing.FatBaseValue = BaseValue;
      existing.FatPendingDamage = PendingDamage;
      existing.FatPendingHealing = PendingHealing;
    }
  }
}
