using System;
using System.Collections.Generic;
using System.Linq;
using Csla;
using Threa.Dal;

namespace GameMechanics.Player
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

    [CreateChild]
    private void Create(string name)
    {
      Name = name;
      Value = 10 + Dice.Roll(4, "F");
    }

    [FetchChild]
    private void Fetch(IAttribute attribute)
    {
      using (BypassPropertyChecks)
      {
        Name = attribute.Name;
        Value = attribute.BaseValue;
      }
    }

    [InsertChild]
    [UpdateChild]
    private void InsertUpdate(List<IAttribute> attributes)
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
    private void Delete(List<IAttribute> attributes)
    {
      if (IsNew) return;
      attributes.Remove(attributes.Where(r => r.Name == Name).First());
    }
  }
}
