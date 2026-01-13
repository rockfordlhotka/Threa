using System;
using System.Collections.Generic;
using System.Linq;
using Csla;
using Threa.Dal.Dto;

namespace GameMechanics
{
  [Serializable]
  public class AttributeEdit : BusinessBase<AttributeEdit>
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
      set => SetProperty(ValueProperty, value);
    }

    public static readonly PropertyInfo<int> BaseValueProperty = RegisterProperty<int>(nameof(BaseValue));
    public int BaseValue
    {
      get => GetProperty(BaseValueProperty);
      set => SetProperty(BaseValueProperty, value);
    }

    public static readonly PropertyInfo<int> SpeciesModifierProperty = RegisterProperty<int>(nameof(SpeciesModifier));
    public int SpeciesModifier
    {
      get => GetProperty(SpeciesModifierProperty);
      private set => LoadProperty(SpeciesModifierProperty, value);
    }

    /// <summary>
    /// Updates the species modifier and recalculates the final value.
    /// This is called when the character's species changes.
    /// </summary>
    internal void UpdateSpeciesModifier(int newModifier)
    {
      using (BypassPropertyChecks)
      {
        LoadProperty(SpeciesModifierProperty, newModifier);
      }
      // Trigger recalculation of Value
      BusinessRules.CheckRules();
    }

    /// <summary>
    /// Rerolls the base value for this attribute. Should only be called during character creation.
    /// </summary>
    internal void RerollBaseValue()
    {
      using (BypassPropertyChecks)
      {
        // Roll base value: 4dF + 10
        LoadProperty(BaseValueProperty, 10 + Dice.Roll(4, "F"));
      }
      // Trigger recalculation of Value
      BusinessRules.CheckRules();
    }

    protected override void AddBusinessRules()
    {
      base.AddBusinessRules();
      // Recalculate Value when BaseValue changes
      BusinessRules.AddRule(new RecalculateValue());
    }

    private class RecalculateValue : Csla.Rules.BusinessRule
    {
      public RecalculateValue() : base(BaseValueProperty)
      {
        InputProperties.Add(BaseValueProperty);
        InputProperties.Add(SpeciesModifierProperty);
        AffectedProperties.Add(ValueProperty);
      }

      protected override void Execute(Csla.Rules.IRuleContext context)
      {
        var target = (AttributeEdit)context.Target;
        var newValue = target.BaseValue + target.SpeciesModifier;
        context.AddOutValue(ValueProperty, newValue);
      }
    }

    [CreateChild]
    private void Create(string name)
    {
      Create(name, 0);
    }

    [CreateChild]
    private void Create(string name, int speciesModifier)
    {
      using (BypassPropertyChecks)
      {
        Name = name;
        LoadProperty(SpeciesModifierProperty, speciesModifier);
        // Roll base value: 4dF + 10 (no modifier)
        LoadProperty(BaseValueProperty, 10 + Dice.Roll(4, "F"));
        // Apply species modifier to get actual value
        LoadProperty(ValueProperty, BaseValue + SpeciesModifier);
      }
    }

    [FetchChild]
    private void Fetch(CharacterAttribute attribute)
    {
      Fetch(attribute, null);
    }

    [FetchChild]
    private void Fetch(CharacterAttribute attribute, Reference.SpeciesInfo? species)
    {
      using (BypassPropertyChecks)
      {
        Name = attribute.Name;
        // Load the base value (without species modifier)
        LoadProperty(BaseValueProperty, attribute.BaseValue);
        // Load or calculate species modifier
        LoadProperty(SpeciesModifierProperty, species?.GetModifier(Name) ?? 0);
        // Calculate final value
        LoadProperty(ValueProperty, attribute.Value);
      }
    }

    [InsertChild]
    [UpdateChild]
    private void InsertUpdate(List<CharacterAttribute> attributes)
    {
      using (BypassPropertyChecks)
      {
        // Find or create the attribute entry
        var item = attributes.FirstOrDefault(r => r.Name == Name);
        if (item == null)
        {
          item = new CharacterAttribute
          {
            Name = Name
          };
          attributes.Add(item);
        }
        
        // Save both base value and final calculated value
        item.BaseValue = BaseValue;
        item.Value = Value;
      }
    }

    [DeleteSelfChild]
    private void Delete(List<CharacterAttribute> attributes)
    {
      if (IsNew) return;
      attributes.Remove(attributes.Where(r => r.Name == Name).First());
    }
  }
}
