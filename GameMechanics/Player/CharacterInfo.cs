using Csla;
using System;
using Threa.Dal.Dto;

namespace GameMechanics.Player
{
  [Serializable]
  public class CharacterInfo : ReadOnlyBase<CharacterInfo>
  {
    public static readonly PropertyInfo<int> IdProperty = RegisterProperty<int>(nameof(Id));
    public int Id
    {
      get => GetProperty(IdProperty);
      private set => LoadProperty(IdProperty, value);
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

    public static readonly PropertyInfo<string> SettingProperty = RegisterProperty<string>(nameof(Setting));
    public string Setting
    {
      get => GetProperty(SettingProperty);
      private set => LoadProperty(SettingProperty, value);
    }

    public static readonly PropertyInfo<Guid?> TableIdProperty = RegisterProperty<Guid?>(nameof(TableId));
    public Guid? TableId
    {
      get => GetProperty(TableIdProperty);
      private set => LoadProperty(TableIdProperty, value);
    }

    public static readonly PropertyInfo<string?> TableNameProperty = RegisterProperty<string?>(nameof(TableName));
    public string? TableName
    {
      get => GetProperty(TableNameProperty);
      private set => LoadProperty(TableNameProperty, value);
    }

    public bool IsAttachedToTable => TableId.HasValue;

    [FetchChild]
    private void Fetch(Character character, GameTable? table)
    {
      Id = character.Id;
      Name = character.Name;
      Species = character.Species;
      IsPlayable = character.IsPlayable;
      Setting = character.Setting;
      
      if (table != null)
      {
        TableId = table.Id;
        TableName = table.Name;
      }
    }
  }
}
