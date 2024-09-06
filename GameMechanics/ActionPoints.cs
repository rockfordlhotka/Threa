using Csla;
using System;
using Threa.Dal.Dto;

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
      var character = (CharacterEdit)Parent;
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
      var character = (CharacterEdit)Parent;
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
      var character = (CharacterEdit)Parent;
      character.Fatigue.PendingDamage += boost;
      Available -= 2;
      Spent += 2;
    }

    public static int CalculateRecovery(int fatigue)
    {
      return fatigue / 4;
    }

    public static int CalculateMax(double xpTotal)
    {
      var result = (int)xpTotal / 10;
      if (result < 1)
        result = 1;
      return result;
    }

    [CreateChild]
    private void Create(CharacterEdit character)
    {
      Recovery = CalculateRecovery(character.Fatigue.BaseValue);
      Max = CalculateMax(character.XPTotal);
      Available = Max;
    }

    [FetchChild]
    private void Fetch(Character character)
    {
      if (character.ActionPointMax == 0)
      {
        Recovery = CalculateRecovery(14);
        Max = CalculateMax(45);
        Available = Max;
      }
      else
      {
        using (BypassPropertyChecks)
        {
          Recovery = character.ActionPointRecovery;
          Max = character.ActionPointMax;
          Available = character.ActionPointAvailable;
        }
      }
    }
  }
}
