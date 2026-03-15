using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Helix.Client.Web.Services;

public class ApiAuthenticationStateProvider(AuthService authService) : AuthenticationStateProvider
{
    private readonly AuthService _authService = authService;
    private readonly AuthenticationState _anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _authService.GetTokenAsync();

        if (string.IsNullOrEmpty(token))
        {
            return _anonymous;
        }

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, token)
        }, "jwt");
        
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public void MarkUserAsAuthenticated()
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "User") });
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void MarkUserAsLoggedOut()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
    }
    
}