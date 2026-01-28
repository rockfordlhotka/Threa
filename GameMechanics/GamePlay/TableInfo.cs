using Csla;
using System;
using Threa.Dal.Dto;

namespace GameMechanics.GamePlay;

/// <summary>
/// Read-only information about a game table for display in lists.
/// </summary>
[Serializable]
public class TableInfo : ReadOnlyBase<TableInfo>
{
    public static readonly PropertyInfo<Guid> IdProperty = RegisterProperty<Guid>(nameof(Id));
    public Guid Id
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

    public static readonly PropertyInfo<int> GameMasterIdProperty = RegisterProperty<int>(nameof(GameMasterId));
    public int GameMasterId
    {
        get => GetProperty(GameMasterIdProperty);
        private set => LoadProperty(GameMasterIdProperty, value);
    }

    public static readonly PropertyInfo<TableStatus> StatusProperty = RegisterProperty<TableStatus>(nameof(Status));
    public TableStatus Status
    {
        get => GetProperty(StatusProperty);
        private set => LoadProperty(StatusProperty, value);
    }

    public static readonly PropertyInfo<int> CurrentRoundProperty = RegisterProperty<int>(nameof(CurrentRound));
    public int CurrentRound
    {
        get => GetProperty(CurrentRoundProperty);
        private set => LoadProperty(CurrentRoundProperty, value);
    }

    public static readonly PropertyInfo<bool> IsInCombatProperty = RegisterProperty<bool>(nameof(IsInCombat));
    public bool IsInCombat
    {
        get => GetProperty(IsInCombatProperty);
        private set => LoadProperty(IsInCombatProperty, value);
    }

    public static readonly PropertyInfo<DateTime> CreatedAtProperty = RegisterProperty<DateTime>(nameof(CreatedAt));
    public DateTime CreatedAt
    {
        get => GetProperty(CreatedAtProperty);
        private set => LoadProperty(CreatedAtProperty, value);
    }

    public static readonly PropertyInfo<DateTime?> LastActivityAtProperty = RegisterProperty<DateTime?>(nameof(LastActivityAt));
    public DateTime? LastActivityAt
    {
        get => GetProperty(LastActivityAtProperty);
        private set => LoadProperty(LastActivityAtProperty, value);
    }

    public static readonly PropertyInfo<string> ThemeProperty = RegisterProperty<string>(nameof(Theme));
    /// <summary>
    /// The visual theme for this table ("fantasy" or "scifi").
    /// </summary>
    public string Theme
    {
        get => GetProperty(ThemeProperty);
        private set => LoadProperty(ThemeProperty, value);
    }

    public static readonly PropertyInfo<long> StartTimeSecondsProperty = RegisterProperty<long>(nameof(StartTimeSeconds));
    /// <summary>
    /// The in-game start time in seconds from epoch 0.
    /// </summary>
    public long StartTimeSeconds
    {
        get => GetProperty(StartTimeSecondsProperty);
        private set => LoadProperty(StartTimeSecondsProperty, value);
    }

    public static readonly PropertyInfo<string> DescriptionProperty = RegisterProperty<string>(nameof(Description));
    /// <summary>
    /// Description of the campaign for players browsing.
    /// </summary>
    public string Description
    {
        get => GetProperty(DescriptionProperty);
        private set => LoadProperty(DescriptionProperty, value);
    }

    public string StatusDisplay => Status switch
    {
        TableStatus.Lobby => "Lobby",
        TableStatus.Active => IsInCombat ? "Combat" : "Active",
        TableStatus.Paused => "Paused",
        TableStatus.Ended => "Ended",
        _ => "Unknown"
    };

    [FetchChild]
    private void Fetch(GameTable table)
    {
        Id = table.Id;
        Name = table.Name;
        GameMasterId = table.GameMasterId;
        Status = table.Status;
        CurrentRound = table.CurrentRound;
        IsInCombat = table.IsInCombat;
        CreatedAt = table.CreatedAt;
        LastActivityAt = table.LastActivityAt;
        Theme = table.Theme ?? "fantasy";
        StartTimeSeconds = table.StartTimeSeconds;
        Description = table.Description ?? string.Empty;
    }
}
