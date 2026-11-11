using System.Security.Claims;
using Heroicsoft.NetForge.Abstractions.Auth;
using Microsoft.AspNetCore.Components.Authorization;

namespace Forumz.Sample;

/// <summary>The sample is always signed in, so the forum can be tried without Identity.</summary>
public sealed class SampleUserContext : IUserContextService
{
    public const string UserId = "dev";

    public string? GetCurrentUserId() => UserId;

    public string? GetCurrentUserName() => "Dev User";

    public bool IsAuthenticated() => true;

    public bool IsAdministrator() => true;

    public IReadOnlyList<string> GetRoleNames() => [WellKnownRoles.Administrator];
}

public sealed class SampleAuthState : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, SampleUserContext.UserId),
                new Claim(ClaimTypes.Name, "Dev User"),
                new Claim(ClaimTypes.Role, WellKnownRoles.Administrator),
            ],
            authenticationType: "Sample");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }
}
