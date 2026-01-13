using Csla;
using System;
using Threa.Dal.Dto;

namespace GameMechanics.GameMaster
{
  [Serializable]
  public class GmCharacterInfo : ReadOnlyBase<GmCharacterInfo>
  {
    public static readonly PropertyInfo<int> IdProperty = RegisterProperty<int>(nameof(Id));
    public int Id
    {
      get => GetProperty(IdProperty);
      private set => LoadProperty(IdProperty, value);
    }

    public static readonly PropertyInfo<int> PlayerIdProperty = RegisterProperty<int>(nameof(PlayerId));
    public int PlayerId
    {
      get => GetProperty(PlayerIdProperty);
      private set => LoadProperty(PlayerIdProperty, value);
    }

    public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(nameof(Name));
    public string Name
    {
      get => GetProperty(NameProperty);
      private set => LoadProperty(NameProperty, value);
    }

    public static readonly PropertyInfo<string> SpeciesProperty = RegisterProperty<string>(nameof(Species));
    public string Species
    {
      get => GetProperty(SpeciesProperty);
      private set => LoadProperty(SpeciesProperty, value);
    }

    public static readonly PropertyInfo<bool> IsPlayableProperty = RegisterProperty<bool>(nameof(IsPlayable));
    public bool IsPlayable
    {
      get => GetProperty(IsPlayableProperty);
      private set => LoadProperty(IsPlayableProperty, value);
    }

    public static readonly PropertyInfo<int> XPTotalProperty = RegisterProperty<int>(nameof(XPTotal));
    public int XPTotal
    {
      get => GetProperty(XPTotalProperty);
      private set => LoadProperty(XPTotalProperty, value);
    }

    public static readonly PropertyInfo<int> XPBankedProperty = RegisterProperty<int>(nameof(XPBanked));
    public int XPBanked
    {
      get => GetProperty(XPBankedProperty);
      private set => LoadProperty(XPBankedProperty, value);
    }

    [FetchChild]
    private void Fetch(Character character)
    {
      Id = character.Id;
      PlayerId = character.PlayerId;
      Name = character.Name;
      Species = character.Species;
      IsPlayable = character.IsPlayable;
      XPTotal = character.XPTotal;
      XPBanked = character.XPBanked;
    }
  }
}
