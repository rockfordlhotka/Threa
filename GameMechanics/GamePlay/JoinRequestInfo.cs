using Csla;
using System;
using Threa.Dal.Dto;

namespace GameMechanics.GamePlay;

/// <summary>
/// Read-only info object for displaying a join request.
/// Contains character name, species, table name, and status for UI display.
/// </summary>
[Serializable]
public class JoinRequestInfo : ReadOnlyBase<JoinRequestInfo>
{
    public static readonly PropertyInfo<Guid> IdProperty = RegisterProperty<Guid>(nameof(Id));
    public Guid Id
    {
        get => GetProperty(IdProperty);
        private set => LoadProperty(IdProperty, value);
    }

    public static readonly PropertyInfo<int> CharacterIdProperty = RegisterProperty<int>(nameof(CharacterId));
    public int CharacterId
    {
        get => GetProperty(CharacterIdProperty);
        private set => LoadProperty(CharacterIdProperty, value);
    }

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

    public static readonly PropertyInfo<Guid> TableIdProperty = RegisterProperty<Guid>(nameof(TableId));
    public Guid TableId
    {
        get => GetProperty(TableIdProperty);
        private set => LoadProperty(TableIdProperty, value);
    }

    public static readonly PropertyInfo<string> TableNameProperty = RegisterProperty<string>(nameof(TableName));
    public string TableName
    {
        get => GetProperty(TableNameProperty);
        private set => LoadProperty(TableNameProperty, value);
    }

    public static readonly PropertyInfo<int> PlayerIdProperty = RegisterProperty<int>(nameof(PlayerId));
    public int PlayerId
    {
        get => GetProperty(PlayerIdProperty);
        private set => LoadProperty(PlayerIdProperty, value);
    }

    public static readonly PropertyInfo<JoinRequestStatus> StatusProperty = RegisterProperty<JoinRequestStatus>(nameof(Status));
    public JoinRequestStatus Status
    {
        get => GetProperty(StatusProperty);
        private set => LoadProperty(StatusProperty, value);
    }

    public static readonly PropertyInfo<DateTime> RequestedAtProperty = RegisterProperty<DateTime>(nameof(RequestedAt));
    public DateTime RequestedAt
    {
        get => GetProperty(RequestedAtProperty);
        private set => LoadProperty(RequestedAtProperty, value);
    }

    /// <summary>
    /// Returns true if the request is pending and can be approved/denied.
    /// </summary>
    public bool IsPending => Status == JoinRequestStatus.Pending;

    /// <summary>
    /// Returns true if the request was approved and player can navigate to campaign.
    /// </summary>
    public bool IsApproved => Status == JoinRequestStatus.Approved;

    [FetchChild]
    private void Fetch(JoinRequest request, Character? character, GameTable? table)
    {
        Id = request.Id;
        CharacterId = request.CharacterId;
        CharacterName = character?.Name ?? "Unknown";
        Species = character?.Species ?? "";
        TableId = request.TableId;
        TableName = table?.Name ?? "Unknown";
        PlayerId = request.PlayerId;
        Status = request.Status;
        RequestedAt = request.RequestedAt;
    }
}
