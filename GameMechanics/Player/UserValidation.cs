using Csla.Core;
using Csla;
using System;
using Threa.Dal;

namespace GameMechanics.Player;

[Serializable]
public class UserValidation : CommandBase<UserValidation>
{
    public static readonly PropertyInfo<bool> IsValidProperty = RegisterProperty<bool>(nameof(IsValid));
    public bool IsValid
    {
        get => ReadProperty(IsValidProperty);
        private set => LoadProperty(IsValidProperty, value);
    }

    public static readonly PropertyInfo<MobileList<string>> RolesProperty = RegisterProperty<MobileList<string>>(nameof(Roles));
    public MobileList<string> Roles
    {
        get => ReadProperty(RolesProperty);
        private set => LoadProperty(RolesProperty, value);
    }

    [Execute]
    private void Execute(string username, [Inject] IPlayerDal dal)
    {
        var user = dal.GetPlayerByEmailAsync(username);
        Roles = new MobileList<string>();
    }

    [Execute]
    private void Execute(string username, string password, [Inject] IPlayerDal dal)
    {
        var user = dal.GetPlayerByEmailAsync(username);
        IsValid = (user is not null);
        Roles = new MobileList<string>();
    }
}
