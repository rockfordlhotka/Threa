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
        // The stored value includes species modifiers
        LoadProperty(ValueProperty, attribute.BaseValue);
        // Back out the species modifier to get the base rolled value
        LoadProperty(SpeciesModifierProperty, species?.GetModifier(Name) ?? 0);
        LoadProperty(BaseValueProperty, Value - SpeciesModifier);
      }
    }

    [InsertChild]
    [UpdateChild]
    private void InsertUpdate(List<CharacterAttribute> attributes)
    {
      using (BypassPropertyChecks)
      {
        var item = attributes.Where(r => r.Name == Name).FirstOrDefault();
        if (item == null)
        {
          item = new Threa.Dal.Dto.CharacterAttribute
          {
            Name = Name
          };
          attributes.Add(item);
        }
        item.BaseValue = Value;
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
