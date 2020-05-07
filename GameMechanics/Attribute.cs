using System;
using Csla;

namespace GameMechanics
{
  [Serializable]
  public class Attribute : BusinessBase<Attribute>
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

    [CreateChild]
    private void Create()
    {
      Value = 5;
      BaseValue = 5;
    }
  }
}
