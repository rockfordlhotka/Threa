using Csla;
using System;
using Threa.Dal.Dto;

namespace GameMechanics
{
  [Serializable]
  public class WoundRecord : BusinessBase<WoundRecord>
  {
    public static readonly PropertyInfo<string> LocationProperty = RegisterProperty<string>(nameof(Location));
    public string Location
    {
      get => GetProperty(LocationProperty);
      set => SetProperty(LocationProperty, value);
    }

    public static readonly PropertyInfo<int> LightWoundsProperty = RegisterProperty<int>(nameof(LightWounds));
    public int LightWounds
    {
      get => GetProperty(LightWoundsProperty);
      set => SetProperty(LightWoundsProperty, value);
    }

    public static readonly PropertyInfo<int> SeriousWoundsProperty = RegisterProperty<int>(nameof(SeriousWounds));
    public int SeriousWounds
    {
      get => GetProperty(SeriousWoundsProperty);
      set => SetProperty(SeriousWoundsProperty, value);
    }

    public static readonly PropertyInfo<int> MaxWoundsProperty = RegisterProperty<int>(nameof(MaxWounds));
    public int MaxWounds
    {
      get => GetProperty(MaxWoundsProperty);
      set => SetProperty(MaxWoundsProperty, value);
    }

    public static readonly PropertyInfo<bool> IsCrippledProperty = RegisterProperty<bool>(nameof(IsCrippled));
    public bool IsCrippled
    {
      get => GetProperty(IsCrippledProperty);
      set => SetProperty(IsCrippledProperty, value);
    }

    public static readonly PropertyInfo<bool> IsDestroyedProperty = RegisterProperty<bool>(nameof(IsDestroyed));
    public bool IsDestroyed
    {
      get => GetProperty(IsDestroyedProperty);
      set => SetProperty(IsDestroyedProperty, value);
    }

    public static readonly PropertyInfo<int> RoundsToDamageProperty = RegisterProperty<int>(nameof(RoundsToDamage));
    public int RoundsToDamage
    {
      get => GetProperty(RoundsToDamageProperty);
      set => SetProperty(RoundsToDamageProperty, value);
    }

    public int TotalWounds
    {
      get => LightWounds + SeriousWounds;
    }

    public bool IsDisabled
    {
      get => TotalWounds >= MaxWounds - 1;
    }

    private CharacterEdit Character
    {
      get => ((WoundList)Parent).Character;
    }

    public void EndOfRound()
    {
      if (RoundsToDamage > 0)
      {
        RoundsToDamage -= 1;
        if (RoundsToDamage == 0)
        {
          RoundsToDamage = 20;
          Character.Vitality.PendingDamage += SeriousWounds;
          Character.Fatigue.PendingDamage += SeriousWounds * 2;
          Character.Fatigue.PendingDamage += LightWounds;
        }
      }
    }

    public void TakeWound()
    {
      if (RoundsToDamage == 0)
        RoundsToDamage = 20;
      SeriousWounds += 1;
      if (SeriousWounds == MaxWounds)
        IsCrippled = true;
      if (SeriousWounds >= MaxWounds)
        IsDestroyed = true;
    }

    public void HealWound()
    {
      if (SeriousWounds > 0)
      {
        SeriousWounds -= 1;
        LightWounds += 1;
      }
      else if (LightWounds > 0)
      {
        LightWounds -= 1;
      }
      if (TotalWounds == 0)
        RoundsToDamage = 0;
    }

    [CreateChild]
    private void Create(string location, int max)
    {
      Location = location;
      MaxWounds = max;
    }

    [FetchChild]
    private void Fetch(Wound wound)
    {
      using (BypassPropertyChecks)
      {
        Csla.Data.DataMapper.Map(wound, this);
      }
    }

    [UpdateChild]
    private void Update(Wound wound)
    {
      using (BypassPropertyChecks)
      {
        Csla.Data.DataMapper.Map(this, wound);
      }
    }
  }
}
