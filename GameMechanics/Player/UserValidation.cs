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
    private async Task Execute(string username, string password, [Inject] IPlayerDal dal)
    {
        var player = await dal.GetPlayerByEmailAsync(username, password);
        Principal = GetPrincipal(player);
    }

    private ClaimsPrincipal GetPrincipal(Threa.Dal.Dto.Player user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.GivenName, user.Name),
            new(ClaimTypes.Email, user.Email)
        };
        var identity = new ClaimsIdentity(claims, "password");
        return new ClaimsPrincipal(identity);
    }
}
