using System;
using Csla;

namespace GameMechanics.Player;

[Serializable]
public class AdminUserInfo : ReadOnlyBase<AdminUserInfo>
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
        private set => LoadProperty(NameProperty, value);
    }

    public static readonly PropertyInfo<string> RolesProperty = RegisterProperty<string>(nameof(Roles));
    public string Roles
    {
        get => GetProperty(RolesProperty);
        private set => LoadProperty(RolesProperty, value);
    }

    public static readonly PropertyInfo<bool> IsEnabledProperty = RegisterProperty<bool>(nameof(IsEnabled));
    public bool IsEnabled
    {
        get => GetProperty(IsEnabledProperty);
        private set => LoadProperty(IsEnabledProperty, value);
    }

    [FetchChild]
    private void Fetch(Threa.Dal.Dto.Player data)
    {
        Id = data.Id;
        Email = data.Email;
        Name = data.Name;
        Roles = data.Roles ?? string.Empty;
        IsEnabled = data.IsEnabled;
    }
}
