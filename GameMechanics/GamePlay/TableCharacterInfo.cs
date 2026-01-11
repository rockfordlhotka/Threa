using Csla;
using System;
using Threa.Dal.Dto;

namespace GameMechanics.GamePlay;

/// <summary>
/// Read-only information about a character connected to a table.
/// </summary>
[Serializable]
public class TableCharacterInfo : ReadOnlyBase<TableCharacterInfo>
{
    public static readonly PropertyInfo<Guid> TableIdProperty = RegisterProperty<Guid>(nameof(TableId));
    public Guid TableId
    {
        get => GetProperty(TableIdProperty);
        private set => LoadProperty(TableIdProperty, value);
    }

    public static readonly PropertyInfo<int> CharacterIdProperty = RegisterProperty<int>(nameof(CharacterId));
    public int CharacterId
    {
        get => GetProperty(CharacterIdProperty);
        private set => LoadProperty(CharacterIdProperty, value);
    }

    public static readonly PropertyInfo<int> PlayerIdProperty = RegisterProperty<int>(nameof(PlayerId));
    public int PlayerId
    {
        get => GetProperty(PlayerIdProperty);
        private set => LoadProperty(PlayerIdProperty, value);
    }

    public static readonly PropertyInfo<DateTime> JoinedAtProperty = RegisterProperty<DateTime>(nameof(JoinedAt));
    public DateTime JoinedAt
    {
        get => GetProperty(JoinedAtProperty);
        private set => LoadProperty(JoinedAtProperty, value);
    }

    public static readonly PropertyInfo<ConnectionStatus> ConnectionStatusProperty = RegisterProperty<ConnectionStatus>(nameof(ConnectionStatus));
    public ConnectionStatus ConnectionStatus
    {
        get => GetProperty(ConnectionStatusProperty);
        private set => LoadProperty(ConnectionStatusProperty, value);
    }

    public static readonly PropertyInfo<DateTime?> LastActivityProperty = RegisterProperty<DateTime?>(nameof(LastActivity));
    public DateTime? LastActivity
    {
        get => GetProperty(LastActivityProperty);
        private set => LoadProperty(LastActivityProperty, value);
    }

    // Character details (loaded separately)
    public static readonly PropertyInfo<string> CharacterNameProperty = RegisterProperty<string>(nameof(CharacterName));
    public string CharacterName
    {
        get => GetProperty(CharacterNameProperty);
        private set => LoadProperty(CharacterNameProperty, value);
    }

    public static readonly PropertyInfo<string> SpeciesProperty = RegisterProperty<string>(nameof(Species));
    public string Species
    {
        get => GetProperty(SpeciesProperty);
        private set => LoadProperty(SpeciesProperty, value);
    }

    public static readonly PropertyInfo<int> FatValueProperty = RegisterProperty<int>(nameof(FatValue));
    public int FatValue
    {
        get => GetProperty(FatValueProperty);
        private set => LoadProperty(FatValueProperty, value);
    }

    public static readonly PropertyInfo<int> FatMaxProperty = RegisterProperty<int>(nameof(FatMax));
    public int FatMax
    {
        get => GetProperty(FatMaxProperty);
        private set => LoadProperty(FatMaxProperty, value);
    }

    public static readonly PropertyInfo<int> VitValueProperty = RegisterProperty<int>(nameof(VitValue));
    public int VitValue
    {
        get => GetProperty(VitValueProperty);
        private set => LoadProperty(VitValueProperty, value);
    }

    public static readonly PropertyInfo<int> VitMaxProperty = RegisterProperty<int>(nameof(VitMax));
    public int VitMax
    {
        get => GetProperty(VitMaxProperty);
        private set => LoadProperty(VitMaxProperty, value);
    }

    public static readonly PropertyInfo<int> ActionPointsProperty = RegisterProperty<int>(nameof(ActionPoints));
    public int ActionPoints
    {
        get => GetProperty(ActionPointsProperty);
        private set => LoadProperty(ActionPointsProperty, value);
    }

    public static readonly PropertyInfo<int> ActionPointMaxProperty = RegisterProperty<int>(nameof(ActionPointMax));
    public int ActionPointMax
    {
        get => GetProperty(ActionPointMaxProperty);
        private set => LoadProperty(ActionPointMaxProperty, value);
    }

    public string ConnectionStatusDisplay => ConnectionStatus switch
    {
        ConnectionStatus.Connected => "Connected",
        ConnectionStatus.Disconnected => "Disconnected",
        ConnectionStatus.Away => "Away",
        _ => "Unknown"
    };

    [FetchChild]
    private void Fetch(TableCharacter tableChar, Character? character)
    {
        TableId = tableChar.TableId;
        CharacterId = tableChar.CharacterId;
        PlayerId = tableChar.PlayerId;
        JoinedAt = tableChar.JoinedAt;
        ConnectionStatus = tableChar.ConnectionStatus;
        LastActivity = tableChar.LastActivity;

        if (character != null)
        {
            CharacterName = character.Name;
            Species = character.Species;
            FatValue = character.FatValue;
            FatMax = character.FatBaseValue;
            VitValue = character.VitValue;
            VitMax = character.VitBaseValue;
            ActionPoints = character.ActionPointAvailable;
            ActionPointMax = character.ActionPointMax;
        }
    }
}
