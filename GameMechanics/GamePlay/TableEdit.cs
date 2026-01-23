using Csla;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.GamePlay;

/// <summary>
/// Editable business object for managing game tables.
/// </summary>
[Serializable]
public class TableEdit : BusinessBase<TableEdit>
{
    public static readonly PropertyInfo<Guid> IdProperty = RegisterProperty<Guid>(nameof(Id));
    public Guid Id
    {
        get => GetProperty(IdProperty);
        private set => LoadProperty(IdProperty, value);
    }

    public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(nameof(Name));
    [Required]
    [Display(Name = "Table Name")]
    public string Name
    {
        get => GetProperty(NameProperty);
        set => SetProperty(NameProperty, value);
    }

    public static readonly PropertyInfo<int> GameMasterIdProperty = RegisterProperty<int>(nameof(GameMasterId));
    public int GameMasterId
    {
        get => GetProperty(GameMasterIdProperty);
        private set => LoadProperty(GameMasterIdProperty, value);
    }

    public static readonly PropertyInfo<int?> CampaignIdProperty = RegisterProperty<int?>(nameof(CampaignId));
    public int? CampaignId
    {
        get => GetProperty(CampaignIdProperty);
        set => SetProperty(CampaignIdProperty, value);
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

    public static readonly PropertyInfo<DateTime?> CombatStartedAtProperty = RegisterProperty<DateTime?>(nameof(CombatStartedAt));
    public DateTime? CombatStartedAt
    {
        get => GetProperty(CombatStartedAtProperty);
        private set => LoadProperty(CombatStartedAtProperty, value);
    }

    public static readonly PropertyInfo<DateTime?> LastTimeAdvanceProperty = RegisterProperty<DateTime?>(nameof(LastTimeAdvance));
    public DateTime? LastTimeAdvance
    {
        get => GetProperty(LastTimeAdvanceProperty);
        private set => LoadProperty(LastTimeAdvanceProperty, value);
    }

    public static readonly PropertyInfo<long> StartTimeSecondsProperty = RegisterProperty<long>(nameof(StartTimeSeconds));
    /// <summary>
    /// The in-game start time in seconds from epoch 0.
    /// Set by the GM when creating the table to establish the game world time.
    /// </summary>
    public long StartTimeSeconds
    {
        get => GetProperty(StartTimeSecondsProperty);
        set => SetProperty(StartTimeSecondsProperty, value);
    }

    /// <summary>
    /// The current in-game time in seconds, calculated from start time plus elapsed rounds.
    /// Each round is 3 seconds.
    /// </summary>
    public long CurrentTimeSeconds => StartTimeSeconds + (CurrentRound * 3L);

    public static readonly PropertyInfo<string> ThemeProperty = RegisterProperty<string>(nameof(Theme));
    /// <summary>
    /// The visual theme for this table ("fantasy" or "scifi").
    /// Controls the overall look and feel of the UI for all players at this table.
    /// </summary>
    public string Theme
    {
        get => GetProperty(ThemeProperty);
        set => SetProperty(ThemeProperty, value);
    }

    public string StatusDisplay => Status switch
    {
        TableStatus.Lobby => "Lobby",
        TableStatus.Active => IsInCombat ? "Combat" : "Active",
        TableStatus.Paused => "Paused",
        TableStatus.Ended => "Ended",
        _ => "Unknown"
    };

    /// <summary>
    /// Starts the table session, transitioning from Lobby to Active.
    /// </summary>
    public void StartSession()
    {
        if (Status == TableStatus.Lobby)
        {
            Status = TableStatus.Active;
            CurrentRound = 1;
        }
    }

    /// <summary>
    /// Pauses the table session.
    /// </summary>
    public void PauseSession()
    {
        if (Status == TableStatus.Active)
        {
            Status = TableStatus.Paused;
        }
    }

    /// <summary>
    /// Resumes a paused session.
    /// </summary>
    public void ResumeSession()
    {
        if (Status == TableStatus.Paused)
        {
            Status = TableStatus.Active;
        }
    }

    /// <summary>
    /// Ends the table session.
    /// </summary>
    public void EndSession()
    {
        Status = TableStatus.Ended;
        IsInCombat = false;
    }

    /// <summary>
    /// Enters combat mode.
    /// </summary>
    public void EnterCombat()
    {
        if (Status == TableStatus.Active && !IsInCombat)
        {
            IsInCombat = true;
            CombatStartedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Exits combat mode.
    /// </summary>
    public void ExitCombat()
    {
        if (IsInCombat)
        {
            IsInCombat = false;
            CombatStartedAt = null;
        }
    }

    /// <summary>
    /// Advances the round counter.
    /// </summary>
    public void AdvanceRound()
    {
        if (Status == TableStatus.Active)
        {
            CurrentRound++;
            LastTimeAdvance = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Advances multiple rounds at once.
    /// </summary>
    public void AdvanceRounds(int count)
    {
        if (Status == TableStatus.Active && count > 0)
        {
            CurrentRound += count;
            LastTimeAdvance = DateTime.UtcNow;
        }
    }

    [Create]
    [RunLocal]
    private void Create([Inject] ApplicationContext applicationContext)
    {
        var ci = (ClaimsIdentity?)applicationContext.User.Identity ??
            throw new InvalidOperationException("User not authenticated");
        var playerId = int.Parse(ci.Claims.First(r => r.Type == ClaimTypes.NameIdentifier).Value);

        using (BypassPropertyChecks)
        {
            Id = Guid.NewGuid();
            GameMasterId = playerId;
            CreatedAt = DateTime.UtcNow;
            Status = TableStatus.Lobby;
            CurrentRound = 0;
            IsInCombat = false;
            Theme = "fantasy";
        }
        BusinessRules.CheckRules();
    }

    [Fetch]
    private async Task FetchAsync(Guid id, [Inject] ITableDal dal)
    {
        var existing = await dal.GetTableAsync(id);
        using (BypassPropertyChecks)
        {
            MapFromDto(existing);
        }
        BusinessRules.CheckRules();
    }

    [Insert]
    private async Task InsertAsync([Inject] ITableDal dal)
    {
        var toSave = dal.GetBlank();
        using (BypassPropertyChecks)
        {
            MapToDto(toSave);
        }
        var result = await dal.SaveTableAsync(toSave);
        Id = result.Id;
    }

    [Update]
    private async Task UpdateAsync([Inject] ITableDal dal)
    {
        using (BypassPropertyChecks)
        {
            var existing = await dal.GetTableAsync(Id);
            MapToDto(existing);
            await dal.SaveTableAsync(existing);
        }
    }

    [Delete]
    private async Task DeleteAsync(Guid id, [Inject] ITableDal dal)
    {
        await dal.DeleteTableAsync(id);
    }

    private void MapFromDto(GameTable dto)
    {
        Id = dto.Id;
        Name = dto.Name;
        GameMasterId = dto.GameMasterId;
        CampaignId = dto.CampaignId;
        CreatedAt = dto.CreatedAt;
        LastActivityAt = dto.LastActivityAt;
        Status = dto.Status;
        CurrentRound = dto.CurrentRound;
        IsInCombat = dto.IsInCombat;
        CombatStartedAt = dto.CombatStartedAt;
        LastTimeAdvance = dto.LastTimeAdvance;
        StartTimeSeconds = dto.StartTimeSeconds;
        Theme = dto.Theme ?? "fantasy";
    }

    private void MapToDto(GameTable dto)
    {
        dto.Id = Id;
        dto.Name = Name;
        dto.GameMasterId = GameMasterId;
        dto.CampaignId = CampaignId;
        dto.CreatedAt = CreatedAt;
        dto.LastActivityAt = LastActivityAt;
        dto.Status = Status;
        dto.CurrentRound = CurrentRound;
        dto.IsInCombat = IsInCombat;
        dto.CombatStartedAt = CombatStartedAt;
        dto.LastTimeAdvance = LastTimeAdvance;
        dto.StartTimeSeconds = StartTimeSeconds;
        dto.Theme = Theme;
    }
}
