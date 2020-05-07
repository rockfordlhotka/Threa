using Csla;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMechanics
{
  [Serializable]
  public class ActionPoints : BusinessBase<ActionPoints>
  {
    public static readonly PropertyInfo<int> AvailableProperty = RegisterProperty<int>(nameof(Available));
    public int Available
    {
      get => GetProperty(AvailableProperty);
      set => SetProperty(AvailableProperty, value);
    }

    public static readonly PropertyInfo<int> LockedProperty = RegisterProperty<int>(nameof(Locked));
    public int Locked
    {
      get => GetProperty(LockedProperty);
      set => SetProperty(LockedProperty, value);
    }

    public static readonly PropertyInfo<int> SpentProperty = RegisterProperty<int>(nameof(Spent));
    public int Spent
    {
      get => GetProperty(SpentProperty);
      set => SetProperty(SpentProperty, value);
    }

    public static readonly PropertyInfo<int> MaxProperty = RegisterProperty<int>(nameof(Max));
    public int Max
    {
      get => GetProperty(MaxProperty);
      set => SetProperty(MaxProperty, value);
    }

    public static readonly PropertyInfo<int> RecoveryProperty = RegisterProperty<int>(nameof(Recovery));
    public int Recovery
    {
      get => GetProperty(RecoveryProperty);
      set => SetProperty(RecoveryProperty, value);
    }

    public void EndOfRound()
    {
      Available += Recovery + Locked;
      Spent = 0;
      Locked = 0;
      if (Available > Max)
        Available = Max;
    }

    public void Rest(int points)
    {
      if (Available < points)
        throw new InvalidOperationException("Insufficient AP");
      if (Spent > 0)
        throw new InvalidOperationException("Action already taken");
      var character = (Character)Parent;
      character.Fatigue.PendingHealing += points;
      Available -= points;
      Locked += Available;
      Available = 0;
    }

    public void TakeActionWithFatigue()
    {
      TakeActionWithFatigue(0);
    }

    public void TakeActionWithFatigue(int boost)
    {
      if (Available < 1)
        throw new InvalidOperationException("Insufficient AP");
      var character = (Character)Parent;
      character.Fatigue.PendingDamage += 1 + boost;
      Available -= 1;
      Spent += 1;
    }

    public void TakeActionNoFatigue()
    {
      TakeActionNoFatigue(0);
    }

    public void TakeActionNoFatigue(int boost)
    {
      if (Available < 2)
        throw new InvalidOperationException("Insufficient AP");
      var character = (Character)Parent;
      character.Fatigue.PendingDamage += boost;
      Available -= 2;
      Spent += 2;
    }

    [CreateChild]
    private void Create(Character character)
    {
      Recovery = character.Fatigue.BaseValue / 4;
      Max = (int)character.XPTotal / 10;
      Available = Max;
    }
  }
}
