using Csla;
using System;
using System.Threading.Tasks;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.GamePlay;

/// <summary>
/// Command object to cancel (withdraw) a pending join request.
/// Only the player who submitted the request can cancel it.
/// </summary>
[Serializable]
public class JoinRequestCanceller : CommandBase<JoinRequestCanceller>
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

    [Execute]
    private async Task ExecuteAsync(
        Guid requestId,
        int playerId,
        [Inject] IJoinRequestDal joinRequestDal)
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

            // 2. Verify player owns this request
            if (request.PlayerId != playerId)
            {
                SetError("You can only cancel your own requests.");
                return;
            }

            // 3. Verify request is still pending
            if (request.Status != JoinRequestStatus.Pending)
            {
                SetError("This request has already been processed.");
                return;
            }

            // 4. Delete the request
            await joinRequestDal.DeleteRequestAsync(requestId);

            LoadProperty(SuccessProperty, true);
        }
        catch (Exception ex)
        {
            SetError($"Failed to cancel request: {ex.Message}");
        }
    }

    private void SetError(string message)
    {
        LoadProperty(SuccessProperty, false);
        LoadProperty(ErrorMessageProperty, message);
    }
}
