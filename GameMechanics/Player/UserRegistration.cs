using System;
using System.Linq;
using System.Threading.Tasks;
using Csla;
using Csla.Rules.CommonRules;
using Threa.Dal;

namespace GameMechanics.Player;

/// <summary>
/// CSLA business object for user self-registration.
/// First registered user automatically becomes Administrator.
/// </summary>
[Serializable]
public class UserRegistration : BusinessBase<UserRegistration>
{
    #region Properties

    public static readonly PropertyInfo<int> IdProperty = RegisterProperty<int>(nameof(Id));
    public int Id
    {
        get => GetProperty(IdProperty);
        private set => LoadProperty(IdProperty, value);
    }

    public static readonly PropertyInfo<string> UsernameProperty = RegisterProperty<string>(nameof(Username));
    public string Username
    {
        get => GetProperty(UsernameProperty);
        set => SetProperty(UsernameProperty, value);
    }

    public static readonly PropertyInfo<string> PasswordProperty = RegisterProperty<string>(nameof(Password));
    public string Password
    {
        get => GetProperty(PasswordProperty);
        set => SetProperty(PasswordProperty, value);
    }

    public static readonly PropertyInfo<string> SecretQuestionProperty = RegisterProperty<string>(nameof(SecretQuestion));
    public string SecretQuestion
    {
        get => GetProperty(SecretQuestionProperty);
        set => SetProperty(SecretQuestionProperty, value);
    }

    public static readonly PropertyInfo<string> SecretAnswerProperty = RegisterProperty<string>(nameof(SecretAnswer));
    public string SecretAnswer
    {
        get => GetProperty(SecretAnswerProperty);
        set => SetProperty(SecretAnswerProperty, value);
    }

    #endregion

    #region Business Rules

    protected override void AddBusinessRules()
    {
        base.AddBusinessRules();

        // Required fields
        BusinessRules.AddRule(new Required(UsernameProperty) { MessageText = "Username is required" });
        BusinessRules.AddRule(new Required(PasswordProperty) { MessageText = "Password is required" });
        BusinessRules.AddRule(new Required(SecretQuestionProperty) { MessageText = "Secret question is required" });
        BusinessRules.AddRule(new Required(SecretAnswerProperty) { MessageText = "Secret answer is required" });

        // Username constraints
        BusinessRules.AddRule(new MinLength(UsernameProperty, 3) { MessageText = "Username must be at least 3 characters" });
        BusinessRules.AddRule(new MaxLength(UsernameProperty, 50) { MessageText = "Username cannot exceed 50 characters" });
        BusinessRules.AddRule(new RegExMatch(UsernameProperty, @"^\S+$") { MessageText = "Username cannot contain spaces" });

        // Password minimum length
        BusinessRules.AddRule(new MinLength(PasswordProperty, 6) { MessageText = "Password must be at least 6 characters" });
    }

    #endregion

    #region Data Portal Operations

    [Create]
    private void Create()
    {
        using (BypassPropertyChecks)
        {
            Username = string.Empty;
            Password = string.Empty;
            SecretQuestion = string.Empty;
            SecretAnswer = string.Empty;
        }
        BusinessRules.CheckRules();
    }

    [Insert]
    private async Task Insert([Inject] IPlayerDal dal)
    {
        // Check for duplicate username (case-insensitive)
        var existing = await dal.GetPlayerByEmailAsync(Username);
        if (existing != null)
            throw new DuplicateKeyException($"Username '{Username}' is already registered");

        // Determine if this is the first user (becomes Admin)
        var allPlayers = await dal.GetAllPlayersAsync();
        bool isFirstUser = !allPlayers.Any();

        // Hash password with BCrypt
        var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(Password, salt);

        // Create player with appropriate role
        var player = new Threa.Dal.Dto.Player
        {
            Email = Username,           // Email field stores username (legacy naming)
            Name = Username,            // Initial display name = username
            Salt = salt,
            HashedPassword = hashedPassword,
            SecretQuestion = SecretQuestion,
            SecretAnswer = SecretAnswer.Trim().ToLowerInvariant(),
            Roles = isFirstUser ? Roles.Administrator : Roles.Player,
            IsEnabled = true
        };

        var result = await dal.SavePlayerAsync(player);
        LoadProperty(IdProperty, result.Id);
    }

    #endregion
}
