using Csla;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMechanics
{
  [Serializable]
  public class Campaign : BusinessBase<Campaign>
  {
    public static readonly PropertyInfo<string> IdProperty = RegisterProperty<string>(nameof(Id));
    public string Id
    {
      get => GetProperty(IdProperty);
      private set => LoadProperty(IdProperty, value);
    }
  }
}
