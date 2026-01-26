using System;
using System.Threading.Tasks;
using Csla;
using Threa.Dal;

namespace GameMechanics.Player;

/// <summary>
/// CSLA command for password recovery workflow.
/// Step 1: GetSecretQuestion - Fetches question for username
/// Step 2: ValidateAnswer - Validates secret answer
/// Step 3: ResetPassword - Sets new password
/// </summary>
[Serializable]
public class PasswordRecovery : CommandBase<PasswordRecovery>
{
    #region Properties

    public static readonly PropertyInfo<string> UsernameProperty = RegisterProperty<string>(nameof(Username));
    public string Username
    {
        get => ReadProperty(UsernameProperty);
        set => LoadProperty(UsernameProperty, value);
    }

    public static readonly PropertyInfo<string> SecretQuestionProperty = RegisterProperty<string>(nameof(SecretQuestion));
    public string SecretQuestion
    {
        get => ReadProperty(SecretQuestionProperty);
        private set => LoadProperty(SecretQuestionProperty, value);
    }

    public static readonly PropertyInfo<string> SecretAnswerProperty = RegisterProperty<string>(nameof(SecretAnswer));
    public string SecretAnswer
    {
        get => ReadProperty(SecretAnswerProperty);
        set => LoadProperty(SecretAnswerProperty, value);
    }

    public static readonly PropertyInfo<string> NewPasswordProperty = RegisterProperty<string>(nameof(NewPassword));
    public string NewPassword
    {
        get => ReadProperty(NewPasswordProperty);
        set => LoadProperty(NewPasswordProperty, value);
    }

    public static readonly PropertyInfo<bool> IsAnswerValidProperty = RegisterProperty<bool>(nameof(IsAnswerValid));
    public bool IsAnswerValid
    {
        get => ReadProperty(IsAnswerValidProperty);
        private set => LoadProperty(IsAnswerValidProperty, value);
    }

    public static readonly PropertyInfo<bool> IsLockedOutProperty = RegisterProperty<bool>(nameof(IsLockedOut));
    public bool IsLockedOut
    {
        get => ReadProperty(IsLockedOutProperty);
        private set => LoadProperty(IsLockedOutProperty, value);
    }

    public static readonly PropertyInfo<int> RemainingAttemptsProperty = RegisterProperty<int>(nameof(RemainingAttempts));
    public int RemainingAttempts
    {
        get => ReadProperty(RemainingAttemptsProperty);
        private set => LoadProperty(RemainingAttemptsProperty, value);
    }

    public static readonly PropertyInfo<bool> PasswordResetSuccessProperty = RegisterProperty<bool>(nameof(PasswordResetSuccess));
    public bool PasswordResetSuccess
    {
        get => ReadProperty(PasswordResetSuccessProperty);
        private set => LoadProperty(PasswordResetSuccessProperty, value);
    }

    public static readonly PropertyInfo<string> ErrorMessageProperty = RegisterProperty<string>(nameof(ErrorMessage));
    public string ErrorMessage
    {
        get => ReadProperty(ErrorMessageProperty);
        private set => LoadProperty(ErrorMessageProperty, value);
    }

    #endregion

    #region Factory Methods

    public static async Task<PasswordRecovery> GetSecretQuestionAsync(string username, IDataPortal<PasswordRecovery> portal)
    {
        var cmd = await portal.CreateAsync();
        cmd.Username = username;
        return await portal.ExecuteAsync(cmd);
    }

    #endregion

    #region Data Portal Operations

    [Create]
    private void Create()
    {
        // Initialize default values
        Username = string.Empty;
        SecretAnswer = string.Empty;
        NewPassword = string.Empty;
        ErrorMessage = string.Empty;
    }

    [Execute]
    private async Task ExecuteAsync([Inject] IPlayerDal dal)
    {
        // If NewPassword is set, this is Step 3 (reset password)
        if (!string.IsNullOrEmpty(NewPassword))
        {
            await ExecuteResetPasswordAsync(dal);
            return;
        }

        // If SecretAnswer is set, this is Step 2 (validate answer)
        if (!string.IsNullOrEmpty(SecretAnswer))
        {
            await ExecuteValidateAnswerAsync(dal);
            return;
        }

        // Otherwise, this is Step 1 (get secret question)
        await ExecuteGetSecretQuestionAsync(dal);
    }

    private async Task ExecuteGetSecretQuestionAsync(IPlayerDal dal)
    {
        // Check lockout first
        IsLockedOut = await dal.IsRecoveryLockedOutAsync(Username);
        if (IsLockedOut)
        {
            ErrorMessage = "Too many failed attempts. Please try again later.";
            return;
        }

        // Get secret question (returns null for unknown user)
        var question = await dal.GetSecretQuestionAsync(Username);

        // Return generic message even if user not found (prevent enumeration)
        // The UI will show "If that username exists..." message
        SecretQuestion = question ?? string.Empty;
        RemainingAttempts = await dal.GetRemainingRecoveryAttemptsAsync(Username);
    }

    private async Task ExecuteValidateAnswerAsync(IPlayerDal dal)
    {
        // Check lockout first
        IsLockedOut = await dal.IsRecoveryLockedOutAsync(Username);
        if (IsLockedOut)
        {
            ErrorMessage = "Too many failed attempts. Please try again later.";
            return;
        }

        // Validate answer
        IsAnswerValid = await dal.ValidateSecretAnswerAsync(Username, SecretAnswer);
        RemainingAttempts = await dal.GetRemainingRecoveryAttemptsAsync(Username);

        if (!IsAnswerValid)
        {
            // Check if now locked out
            IsLockedOut = await dal.IsRecoveryLockedOutAsync(Username);
            if (IsLockedOut)
            {
                ErrorMessage = "Too many failed attempts. Please try again later.";
            }
            else
            {
                ErrorMessage = $"Incorrect answer. You have {RemainingAttempts} attempt(s) remaining.";
            }
        }
    }

    private async Task ExecuteResetPasswordAsync(IPlayerDal dal)
    {
        // Validate password length
        if (string.IsNullOrEmpty(NewPassword) || NewPassword.Length < 6)
        {
            ErrorMessage = "Password must be at least 6 characters.";
            PasswordResetSuccess = false;
            return;
        }

        try
        {
            await dal.ResetPasswordAsync(Username, NewPassword);
            PasswordResetSuccess = true;
        }
        catch (NotFoundException)
        {
            ErrorMessage = "Password reset failed. Please try again.";
            PasswordResetSuccess = false;
        }
    }

    #endregion
}
