using Csla;
using GameMechanics.Effects;
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

    public void EndOfRound(Csla.IChildDataPortal<EffectRecord>? effectPortal = null)
    {
      // Note: VIT does NOT have passive recovery like FAT does
      // VIT healing comes from spells, potions, or hourly recovery

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
        // cascade overflow to wounds
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

    internal void CalculateBase(CharacterEdit character)
    {
      BaseValue = character.GetAttribute("STR") * 2 - 5;
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
