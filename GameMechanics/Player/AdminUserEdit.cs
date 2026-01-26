using System;
using System.Linq;
using System.Threading.Tasks;
using Csla;
using Csla.Core;
using Csla.Rules;
using Microsoft.Extensions.DependencyInjection;
using Threa.Dal;

namespace GameMechanics.Player;

[Serializable]
public class AdminUserEdit : BusinessBase<AdminUserEdit>
{
    public static readonly PropertyInfo<int> IdProperty = RegisterProperty<int>(nameof(Id));
    public int Id
    {
        get => GetProperty(IdProperty);
        private set => LoadProperty(IdProperty, value);
    }

    public static readonly PropertyInfo<string> EmailProperty = RegisterProperty<string>(nameof(Email));
    public string Email
    {
        get => GetProperty(EmailProperty);
        private set => LoadProperty(EmailProperty, value);
    }

    public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(nameof(Name));
    public string Name
    {
        get => GetProperty(NameProperty);
        set => SetProperty(NameProperty, value);
    }

    public static readonly PropertyInfo<bool> IsEnabledProperty = RegisterProperty<bool>(nameof(IsEnabled));
    public bool IsEnabled
    {
        get => GetProperty(IsEnabledProperty);
        set => SetProperty(IsEnabledProperty, value);
    }

    public static readonly PropertyInfo<bool> IsAdministratorProperty = RegisterProperty<bool>(nameof(IsAdministrator));
    public bool IsAdministrator
    {
        get => GetProperty(IsAdministratorProperty);
        set => SetProperty(IsAdministratorProperty, value);
    }

    public static readonly PropertyInfo<bool> IsGameMasterProperty = RegisterProperty<bool>(nameof(IsGameMaster));
    public bool IsGameMaster
    {
        get => GetProperty(IsGameMasterProperty);
        set => SetProperty(IsGameMasterProperty, value);
    }

    public static readonly PropertyInfo<bool> IsPlayerProperty = RegisterProperty<bool>(nameof(IsPlayer));
    public bool IsPlayer
    {
        get => GetProperty(IsPlayerProperty);
        set => SetProperty(IsPlayerProperty, value);
    }

    public static readonly PropertyInfo<string> NewPasswordProperty = RegisterProperty<string>(nameof(NewPassword));
    public string NewPassword
    {
        get => GetProperty(NewPasswordProperty);
        set => SetProperty(NewPasswordProperty, value);
    }

    [Fetch]
    private async Task Fetch(int id, [Inject] IPlayerDal dal)
    {
        var data = await dal.GetPlayerAsync(id)
            ?? throw new InvalidOperationException($"Player {id} not found");
        LoadProperties(data);
    }

    private void LoadProperties(Threa.Dal.Dto.Player data)
    {
        using (BypassPropertyChecks)
        {
            Id = data.Id;
            Email = data.Email;
            Name = data.Name;
            IsEnabled = data.IsEnabled;
            NewPassword = string.Empty;

            // Parse roles
            var roles = data.Roles?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                ?? Array.Empty<string>();
            IsAdministrator = roles.Contains(Roles.Administrator, StringComparer.OrdinalIgnoreCase);
            IsGameMaster = roles.Contains(Roles.GameMaster, StringComparer.OrdinalIgnoreCase);
            IsPlayer = roles.Contains(Roles.Player, StringComparer.OrdinalIgnoreCase);
        }
        BusinessRules.CheckRules();
    }

    protected override void AddBusinessRules()
    {
        base.AddBusinessRules();
        // Register rule against IsEnabled - triggers when IsEnabled or IsAdministrator changes
        BusinessRules.AddRule(new LastAdminProtectionRule(IsEnabledProperty));
        BusinessRules.AddRule(new LastAdminProtectionRule(IsAdministratorProperty));
    }

    /// <summary>
    /// Validates that disabling user or removing Admin role doesn't leave system without admins.
    /// </summary>
    private class LastAdminProtectionRule : BusinessRuleAsync
    {
        public LastAdminProtectionRule(IPropertyInfo primaryProperty)
            : base(primaryProperty)
        {
            InputProperties.Add(IsEnabledProperty);
            InputProperties.Add(IsAdministratorProperty);
            IsAsync = true;
            ProvideTargetWhenAsync = true;
        }

        protected override async Task ExecuteAsync(IRuleContext context)
        {
            var target = (AdminUserEdit)context.Target!;
            var isEnabled = (bool)context.InputPropertyValues[IsEnabledProperty]!;
            var isAdmin = (bool)context.InputPropertyValues[IsAdministratorProperty]!;

            // Only check if this change would remove an enabled admin
            if (!isEnabled || !isAdmin)
            {
                var dal = target.ApplicationContext.GetRequiredService<IPlayerDal>();
                var enabledAdminCount = await dal.CountEnabledAdminsAsync();

                // Check if this user is currently an enabled admin in the database
                var currentUser = await dal.GetPlayerAsync(target.Id);
                if (currentUser != null)
                {
                    var currentRoles = currentUser.Roles?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                        ?? Array.Empty<string>();
                    var wasEnabledAdmin = currentUser.IsEnabled &&
                        currentRoles.Contains(Roles.Administrator, StringComparer.OrdinalIgnoreCase);

                    // If they were an enabled admin and this change would leave 0 enabled admins, block
                    if (wasEnabledAdmin && enabledAdminCount <= 1)
                    {
                        context.AddErrorResult("System must have at least one enabled administrator.");
                    }
                }
            }
        }
    }

    [Update]
    private async Task Update([Inject] IPlayerDal dal)
    {
        // Build roles string
        var rolesList = new System.Collections.Generic.List<string>();
        if (IsAdministrator) rolesList.Add(Roles.Administrator);
        if (IsGameMaster) rolesList.Add(Roles.GameMaster);
        if (IsPlayer) rolesList.Add(Roles.Player);
        var roles = rolesList.Count > 0 ? string.Join(",", rolesList) : null;

        // Get existing player to preserve password if not changing
        var existing = await dal.GetPlayerAsync(Id)
            ?? throw new InvalidOperationException($"Player {Id} not found");

        var player = new Threa.Dal.Dto.Player
        {
            Id = Id,
            Email = Email,
            Name = Name,
            Roles = roles,
            IsEnabled = IsEnabled,
            Salt = existing.Salt,
            HashedPassword = existing.HashedPassword,
            ImageUrl = existing.ImageUrl
        };

        // If new password is set, hash it
        if (!string.IsNullOrWhiteSpace(NewPassword))
        {
            if (string.IsNullOrWhiteSpace(player.Salt))
                player.Salt = BCrypt.Net.BCrypt.GenerateSalt(12);
            player.HashedPassword = BCrypt.Net.BCrypt.HashPassword(NewPassword, player.Salt);
        }

        await dal.SavePlayerAsync(player);
    }
}
