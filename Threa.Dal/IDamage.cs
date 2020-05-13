using System;
using System.Collections.Generic;
using System.Text;

namespace Threa.Dal
{
  public interface IDamage
  {
    int Value { get; set; }
    int BaseValue { get; set; }
    int PendingHealing { get; set; }
    int PendingDamage { get; set; }
  }
}
