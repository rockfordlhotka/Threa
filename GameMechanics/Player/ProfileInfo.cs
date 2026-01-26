using System;
using System.Threading.Tasks;
using Csla;
using Threa.Dal;

namespace GameMechanics.Player;

/// <summary>
/// Read-only profile info for viewing any user's public profile.
/// Exposes only public-safe fields: Id, Name (display name), ContactEmail, UseGravatar.
/// </summary>
[Serializable]
public class ProfileInfo : ReadOnlyBase<ProfileInfo>
{
    public static readonly PropertyInfo<int> IdProperty = RegisterProperty<int>(nameof(Id));
    public int Id
    {
        get => GetProperty(IdProperty);
        private set => LoadProperty(IdProperty, value);
    }

    public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(nameof(Name));
    public string Name
    {
        get => GetProperty(NameProperty);
        private set => LoadProperty(NameProperty, value);
    }

    public static readonly PropertyInfo<string> ContactEmailProperty = RegisterProperty<string>(nameof(ContactEmail));
    public string ContactEmail
    {
        get => GetProperty(ContactEmailProperty);
        private set => LoadProperty(ContactEmailProperty, value);
    }

    public static readonly PropertyInfo<bool> UseGravatarProperty = RegisterProperty<bool>(nameof(UseGravatar));
    public bool UseGravatar
    {
        get => GetProperty(UseGravatarProperty);
        private set => LoadProperty(UseGravatarProperty, value);
    }

    [Fetch]
    private async Task FetchAsync(int id, [Inject] IPlayerDal dal)
    {
        var data = await dal.GetPlayerAsync(id)
            ?? throw new InvalidOperationException($"Player {id} not found");

        Id = data.Id;
        Name = data.Name;
        ContactEmail = data.ContactEmail;
        UseGravatar = data.UseGravatar;
    }
}
