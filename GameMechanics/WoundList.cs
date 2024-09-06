using Csla;
using GameMechanics.Reference;
using System;
using System.Collections.Generic;
using System.Linq;
using Threa.Dal.Dto;

namespace GameMechanics
{
  [Serializable]
  public class WoundList : BusinessListBase<WoundList, WoundRecord>
  {
    internal CharacterEdit Character
    {
      get => (CharacterEdit)Parent;
    }

    public void EndOfRound()
    {
      foreach (var item in this)
        item.EndOfRound();
    }

    public void TakeWound(int count)
    {
      for (int i = 0; i < count; i++)
      {
        var location = GetLocation();
        var record = this.Where(r => r.Location == location).First();
        record.TakeWound();
      }
    }

    internal void TakeDamage(DamageValue damageValue)
    {
      var dmg = damageValue.GetModifiedDamage(Character.DamageClass);
      if (dmg >= 7 && dmg <= 9)
        TakeWound(1);
      else if (dmg >= 10 && dmg <= 14)
        TakeWound(2);
      else if (dmg >= 15 && dmg <= 19)
        TakeWound(3);
      else if (dmg > 19)
        TakeWound(dmg / 5);
    }

    public string GetLocation()
    {
      var location = Dice.Roll(1, 12);
      if (location == 12)
      {
        location = Dice.Roll(1, 12);
        if (location == 12)
          return "Head";
        else
          return GetLocation();
      }
      else if (location > 9)
      {
        return "RightLeg";
      }
      else if (location > 7)
      {
        return "LeftLeg";
      }
      else if (location > 5)
      {
        return "RightArm";
      }
      else if (location > 3)
      {
        return "LeftArm";
      }
      else
      {
        return "Torso";
      }
    }

    [CreateChild]
    private void Create([Inject] IChildDataPortal<WoundRecord> woundPortal)
    {
      using (LoadListMode)
      {
        Add(woundPortal.CreateChild("Head", 2));
        Add(woundPortal.CreateChild("RightLeg", 2));
        Add(woundPortal.CreateChild("LeftLeg", 2));
        Add(woundPortal.CreateChild("RightArm", 2));
        Add(woundPortal.CreateChild("LeftArm", 2));
        Add(woundPortal.CreateChild("Torso", 4));
      }
    }

    [FetchChild]
    private void Fetch(List<Wound> wounds, [Inject] IChildDataPortal<WoundRecord> woundPortal)
    {
      if (wounds == null) return;
      using (LoadListMode)
      {
        foreach (var item in wounds)
          Add(woundPortal.FetchChild(item));
      }
    }
  }
}
