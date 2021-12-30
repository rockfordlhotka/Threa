using System;
using System.Collections.Generic;
using System.Linq;
using Csla;
using Threa.Dal;

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
      set => LoadProperty(BaseValueProperty, value);
    }

    [CreateChild]
    private void Create(string name)
    {
      Name = name;
      Value = BaseValue = 10 + Dice.Roll(4, "F");
    }

    [FetchChild]
    private void Fetch(ICharacterAttribute attribute)
    {
      using (BypassPropertyChecks)
      {
        Name = attribute.Name;
        Value = attribute.BaseValue;
      }
    }

    [InsertChild]
    [UpdateChild]
    private void InsertUpdate(List<ICharacterAttribute> attributes)
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
    private void Delete(List<ICharacterAttribute> attributes)
    {
      if (IsNew) return;
      attributes.Remove(attributes.Where(r => r.Name == Name).First());
    }
  }
}
