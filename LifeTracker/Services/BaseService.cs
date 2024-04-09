using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace LifeTracker.Services;

public class BaseService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    protected BaseService(AuthenticationStateProvider authenticationStateProvider)
    {
        _authenticationStateProvider = authenticationStateProvider;
    }

    protected async Task<string?> GetCurrentUserIdAsync()
    {
        var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
        return state.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}