using Csla;
using GameMechanics.Messaging;
using System;
using System.Threading.Tasks;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.GamePlay;

/// <summary>
/// Command object to approve or deny a join request.
/// On approval: attaches character to table, updates request status, publishes notification.
/// On denial: publishes notification, then deletes request (per CONTEXT.md - immediate deletion).
/// </summary>
[Serializable]
public class JoinRequestProcessor : CommandBase<JoinRequestProcessor>
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

    public static readonly PropertyInfo<string?> CharacterNameProperty = RegisterProperty<string?>(nameof(CharacterName));
    public string? CharacterName
    {
        get => ReadProperty(CharacterNameProperty);
        private set => LoadProperty(CharacterNameProperty, value);
    }

    [Execute]
    private async Task ExecuteAsync(
        Guid requestId,
        bool approve,
        int gameMasterId,
        [Inject] IJoinRequestDal joinRequestDal,
        [Inject] ITableDal tableDal,
        [Inject] ICharacterDal characterDal,
        [Inject] ITimeEventPublisher timeEventPublisher)
    {
        try
        {
            // 1. Get the request
            var request = await joinRequestDal.GetRequestAsync(requestId);
            if (request == null)
            {
                SetError("Join request not found.");
                return;
            }

            if (request.Status != JoinRequestStatus.Pending)
            {
                SetError("This request has already been processed.");
                return;
            }

            // 2. Verify GM owns the table
            var table = await tableDal.GetTableAsync(request.TableId);
            if (table.GameMasterId != gameMasterId)
            {
                SetError("You are not the Game Master of this campaign.");
                return;
            }

            // 3. Get character name for response
            var character = await characterDal.GetCharacterAsync(request.CharacterId);
            LoadProperty(CharacterNameProperty, character?.Name);

            if (approve)
            {
                // 4a. Approve: verify character still valid, then attach to table
                if (character == null)
                {
                    SetError("Character no longer exists.");
                    await joinRequestDal.DeleteRequestAsync(requestId);
                    return;
                }

                // Double-check character isn't already in another campaign
                var existingTable = await tableDal.GetTableForCharacterAsync(request.CharacterId);
                if (existingTable != null)
                {
                    SetError($"{character.Name} joined another campaign while request was pending.");
                    await joinRequestDal.DeleteRequestAsync(requestId);
                    return;
                }

                // Attach character to table
                var tableCharacter = new TableCharacter
                {
                    TableId = request.TableId,
                    CharacterId = request.CharacterId,
                    PlayerId = request.PlayerId,
                    JoinedAt = DateTime.UtcNow,
                    ConnectionStatus = ConnectionStatus.Disconnected,
                    LastActivity = DateTime.UtcNow
                };
                await tableDal.AddCharacterToTableAsync(tableCharacter);

                // Update request status
                request.Status = JoinRequestStatus.Approved;
                request.ProcessedAt = DateTime.UtcNow;
                await joinRequestDal.SaveRequestAsync(request);

                // Publish approval notification (JOIN-10)
                await timeEventPublisher.PublishJoinRequestAsync(new JoinRequestMessage
                {
                    RequestId = requestId,
                    CharacterId = request.CharacterId,
                    PlayerId = request.PlayerId,
                    TableId = request.TableId,
                    Status = JoinRequestStatus.Approved,
                    CharacterName = character.Name,
                    TableName = table.Name
                });
            }
            else
            {
                // 4b. Deny: publish notification BEFORE delete (JOIN-11)
                await timeEventPublisher.PublishJoinRequestAsync(new JoinRequestMessage
                {
                    RequestId = requestId,
                    CharacterId = request.CharacterId,
                    PlayerId = request.PlayerId,
                    TableId = request.TableId,
                    Status = JoinRequestStatus.Denied,
                    CharacterName = character?.Name,
                    TableName = table.Name
                });

                // Delete the request (per CONTEXT.md - immediate deletion)
                await joinRequestDal.DeleteRequestAsync(requestId);
            }

            LoadProperty(SuccessProperty, true);
        }
        catch (Exception ex)
        {
            SetError($"Failed to process request: {ex.Message}");
        }
    }

    private void SetError(string message)
    {
        LoadProperty(SuccessProperty, false);
        LoadProperty(ErrorMessageProperty, message);
    }
}
