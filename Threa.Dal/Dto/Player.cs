namespace Threa.Dal.Dto;

public class Player
{
    public int Id { get; set; } = -1;
    public string Name { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public string HashedPassword { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? Roles { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string SecretQuestion { get; set; } = string.Empty;
    public string SecretAnswer { get; set; } = string.Empty;  // Stored lowercase, trimmed
    public int FailedRecoveryAttempts { get; set; } = 0;
    public DateTime? RecoveryLockoutUntil { get; set; }
    public string ContactEmail { get; set; } = string.Empty;  // For Gravatar (separate from Email which stores username)
    public bool UseGravatar { get; set; } = true;  // Default to using Gravatar when ContactEmail provided
}
