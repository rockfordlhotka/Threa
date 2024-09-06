using Csla.Core;
using Csla;
using System;
using Threa.Dal;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Collections.Generic;

namespace GameMechanics.Player;

[Serializable]
public class UserValidation : CommandBase<UserValidation>
{
    public static readonly PropertyInfo<ClaimsPrincipal> PrincipalProperty = RegisterProperty<ClaimsPrincipal>(nameof(Principal));
    public ClaimsPrincipal Principal
    {
        get => ReadProperty(PrincipalProperty);
        set => LoadProperty(PrincipalProperty, value);
    }

    [Execute]
    private async Task Execute(string username, [Inject] IPlayerDal dal)
    {
        var user = await dal.GetPlayerByEmailAsync(username);
        if (user == null)
            throw new InvalidOperationException("Invalid username");
        Principal = GetPrincipal(user);
    }

    [Execute]
    private async Task Execute(string username, string password, [Inject] IPlayerDal dal)
    {
        var user = await dal.GetPlayerByEmailAsync(username);
        if (user == null)
            throw new InvalidOperationException("Invalid username or password");
        Principal = GetPrincipal(user);
    }

    private ClaimsPrincipal GetPrincipal(Threa.Dal.Dto.Player user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };
        var identity = new ClaimsIdentity(claims, "password");
        return new ClaimsPrincipal(identity);
    }
}
