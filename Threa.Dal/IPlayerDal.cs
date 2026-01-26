using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal;

public interface IPlayerDal
{
    Task<IEnumerable<Player>> GetAllPlayersAsync();
    Task<Player?> GetPlayerAsync(int id);
    Task<Player?> GetPlayerByEmailAsync(string email);
    Task<Player> GetPlayerByEmailAsync(string email, string password);
    Task<Player> SavePlayerAsync(Player obj);
    Task ChangePassword(int id, string oldPassword, string newPassword);
    Task DeletePlayerAsync(int id);

    /// <summary>
    /// Gets the secret question for a username. Returns null if user not found (prevents enumeration).
    /// </summary>
    Task<string?> GetSecretQuestionAsync(string username);

    /// <summary>
    /// Validates the secret answer. Returns true if correct and not locked out.
    /// Increments failed attempts on wrong answer. Clears attempts on success.
    /// </summary>
    Task<bool> ValidateSecretAnswerAsync(string username, string answer);

    /// <summary>
    /// Resets password for username. Should only be called after successful answer validation.
    /// </summary>
    Task ResetPasswordAsync(string username, string newPassword);

    /// <summary>
    /// Checks if user is currently locked out from recovery attempts.
    /// </summary>
    Task<bool> IsRecoveryLockedOutAsync(string username);

    /// <summary>
    /// Gets the number of remaining recovery attempts before lockout.
    /// </summary>
    Task<int> GetRemainingRecoveryAttemptsAsync(string username);

    /// <summary>
    /// Counts enabled users with Administrator role.
    /// Used by last-admin protection rule.
    /// </summary>
    Task<int> CountEnabledAdminsAsync();
}