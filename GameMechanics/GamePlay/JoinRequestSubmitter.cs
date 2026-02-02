using Csla;
using GameMechanics.Messaging;
using System;
using System.Threading.Tasks;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.GamePlay;

/// <summary>
/// Command object to submit a join request for a character to join a table.
/// Validates character ownership, playability, single-campaign constraint, and duplicate request prevention.
/// </summary>
[Serializable]
public class JoinRequestSubmitter : CommandBase<JoinRequestSubmitter>
{
    public static readonly PropertyInfo<bool> SuccessProperty = RegisterProperty<bool>(nameof(Success));
    public bool Success
    {
        get => ReadProperty(SuccessProperty);
        private set => LoadProperty(SuccessProperty, value);
    }

    public static readonly PropertyInfo<string?> ErrorMessageProperty = RegisterProperty<string?>(nameof(ErrorMessage));
    public string? ErrorMessage
    {
        get => ReadProperty(ErrorMessageProperty);
        private set => LoadProperty(ErrorMessageProperty, value);
    }

    public static readonly PropertyInfo<Guid?> RequestIdProperty = RegisterProperty<Guid?>(nameof(RequestId));
    public Guid? RequestId
    {
        get => ReadProperty(RequestIdProperty);
        private set => LoadProperty(RequestIdProperty, value);
    }

    [Execute]
    private async Task ExecuteAsync(
        int characterId,
        Guid tableId,
        int playerId,
        [Inject] IJoinRequestDal joinRequestDal,
        [Inject] ICharacterDal characterDal,
        [Inject] ITableDal tableDal,
        [Inject] ITimeEventPublisher timeEventPublisher)
    {
        try
        {
            // 1. Validate character exists and belongs to player
            var character = await characterDal.GetCharacterAsync(characterId);
            if (character == null)
            {
                SetError("Character not found.");
                return;
            }
            if (character.PlayerId != playerId)
            {
                SetError("You do not own this character.");
                return;
            }
            if (!character.IsPlayable)
            {
                SetError("This character is not activated for play.");
                return;
            }

            // 2. Validate table exists
            GameTable table;
            try
            {
                table = await tableDal.GetTableAsync(tableId);
            }
            catch (NotFoundException)
            {
                SetError("Campaign not found.");
                return;
            }

            // 3. Check character not already in another campaign (JOIN-09)
            var existingTable = await tableDal.GetTableForCharacterAsync(characterId);
            if (existingTable != null)
            {
                SetError($"{character.Name} is already active in {existingTable.Name}. Leave that campaign first.");
                return;
            }

            // 4. Check for existing pending request (prevent duplicates)
            var existingRequest = await joinRequestDal.GetPendingRequestAsync(characterId, tableId);
            if (existingRequest != null)
            {
                SetError("You already have a pending request for this campaign.");
                return;
            }

            // 5. Create the join request
            var request = joinRequestDal.GetBlank();
            request.CharacterId = characterId;
            request.TableId = tableId;
            request.PlayerId = playerId;
            request.Status = JoinRequestStatus.Pending;
            request.RequestedAt = DateTime.UtcNow;

            var saved = await joinRequestDal.SaveRequestAsync(request);

            // Publish join request message for real-time GM notification
            if (!timeEventPublisher.IsConnected)
                await timeEventPublisher.ConnectAsync();
            await timeEventPublisher.PublishJoinRequestAsync(new JoinRequestMessage
            {
                RequestId = saved.Id,
                CharacterId = characterId,
                PlayerId = playerId,
                TableId = tableId,
                Status = JoinRequestStatus.Pending,
                CharacterName = character.Name,
                TableName = table.Name,
                CampaignId = tableId.ToString(),
                SourceId = playerId.ToString()
            });

            LoadProperty(SuccessProperty, true);
            LoadProperty(RequestIdProperty, saved.Id);
        }
        catch (Exception ex)
        {
            SetError($"Failed to submit join request: {ex.Message}");
        }
    }

    private void SetError(string message)
    {
        LoadProperty(SuccessProperty, false);
        LoadProperty(ErrorMessageProperty, message);
    }
}
