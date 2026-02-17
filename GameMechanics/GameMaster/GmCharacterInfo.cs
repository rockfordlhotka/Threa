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

    public static readonly PropertyInfo<bool> IsNpcProperty = RegisterProperty<bool>(nameof(IsNpc));
    public bool IsNpc
    {
      get => GetProperty(IsNpcProperty);
      private set => LoadProperty(IsNpcProperty, value);
    }

    public static readonly PropertyInfo<bool> IsTemplateProperty = RegisterProperty<bool>(nameof(IsTemplate));
    public bool IsTemplate
    {
      get => GetProperty(IsTemplateProperty);
      private set => LoadProperty(IsTemplateProperty, value);
    }

    public static readonly PropertyInfo<string> SettingProperty = RegisterProperty<string>(nameof(Setting));
    public string Setting
    {
      get => GetProperty(SettingProperty);
      private set => LoadProperty(SettingProperty, value);
    }

    /// <summary>
    /// Gets the character type for display: "PC", "NPC", or "Template"
    /// </summary>
    public string CharacterType
    {
      get
      {
        if (IsTemplate) return "Template";
        if (IsNpc) return "NPC";
        return "PC";
      }
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
      IsNpc = character.IsNpc;
      IsTemplate = character.IsTemplate;
      Setting = character.Setting;
    }
  }
}
