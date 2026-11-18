using System.Security.Claims;
using Heroicsoft.NetForge.Abstractions.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Forumz.Sample;

/// <summary>Accounts the sample can sign in as. Dev User administers the forum. The others are members.</summary>
public static class DemoUsers
{
    public const string SignedOutCookie = "forumz.demo.signed-out";

    public static readonly DemoUser[] All =
    [
        new("dev", "Dev User", true),
        new("blair", "Blair Chen", false),
        new("casey", "Casey Nguyen", false),
    ];

    public static DemoUser Administrator => All[0];

    public static DemoUser? Find(string? id) =>
        All.FirstOrDefault(user => string.Equals(user.Id, id, StringComparison.OrdinalIgnoreCase));

    public static ClaimsPrincipal Principal(DemoUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.DisplayName),
        };

        if (user.IsAdministrator)
        {
            claims.Add(new Claim(ClaimTypes.Role, WellKnownRoles.Administrator));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }

    /// <summary>Points the current scope at a demo user. The sample seeder uses this before the site is serving requests.</summary>
    public static void Impersonate(IHttpContextAccessor accessor, IServiceProvider services, DemoUser user)
    {
        accessor.HttpContext = new DefaultHttpContext
        {
            RequestServices = services,
            User = Principal(user),
        };
    }

    public static string LocalReturn(string? value)
    {
        string path = value?.Trim() ?? string.Empty;
        if (path.Length == 0
            || !path.StartsWith('/')
            || path.StartsWith("//", StringComparison.Ordinal)
            || path.Contains('\\')
            || path.Contains('\r')
            || path.Contains('\n'))
        {
            return "/forums";
        }

        return path;
    }
}

public sealed record DemoUser(string Id, string DisplayName, bool IsAdministrator);

public static class DemoAuth
{
    public static IServiceCollection AddDemoUsers(this IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "forumz.demo";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.LoginPath = "/forums";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(14);
            });

        services.AddAuthorization();
        services.AddCascadingAuthenticationState();
        services.AddHttpContextAccessor();
        return services;
    }

    public static WebApplication UseDemoSignIn(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            // The first visit has no cookie. Sign in as the administrator and redirect so the
            // browser stores the cookie before the forum circuit starts.
            if (context.User.Identity?.IsAuthenticated != true
                && !context.Request.Cookies.ContainsKey(DemoUsers.SignedOutCookie)
                && IsForumPage(context.Request))
            {
                await context.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    DemoUsers.Principal(DemoUsers.Administrator));
                context.Response.Redirect(context.Request.PathBase + context.Request.Path + context.Request.QueryString);
                return;
            }

            await next(context);
        });

        return app;
    }

    public static WebApplication MapDemoAuth(this WebApplication app)
    {
        app.MapPost("/account/sign-in", (Delegate)SignIn).DisableAntiforgery();
        app.MapPost("/account/sign-out", (Delegate)SignOut).DisableAntiforgery();
        return app;
    }

    private static bool IsForumPage(HttpRequest request)
    {
        if (!HttpMethods.IsGet(request.Method))
        {
            return false;
        }

        string path = request.Path.Value ?? string.Empty;
        bool forumPage = path == "/"
            || path.Equals("/forums", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/forums/", StringComparison.OrdinalIgnoreCase);
        if (!forumPage || path.Contains("/_", StringComparison.Ordinal))
        {
            return false;
        }

        return request.Headers.Accept.Any(value =>
            value?.Contains("text/html", StringComparison.OrdinalIgnoreCase) == true);
    }

    private static async Task<IResult> SignIn(HttpContext http)
    {
        string? userId = http.Request.HasFormContentType ? http.Request.Form["userId"].ToString() : null;
        var user = DemoUsers.Find(userId);
        if (user is null)
        {
            return Results.BadRequest();
        }

        http.Response.Cookies.Delete(DemoUsers.SignedOutCookie, BaseCookie(http));
        await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, DemoUsers.Principal(user));
        string? returnUrl = http.Request.HasFormContentType ? http.Request.Form["returnUrl"].ToString() : null;
        return Results.LocalRedirect(DemoUsers.LocalReturn(returnUrl));
    }

    private static async Task<IResult> SignOut(HttpContext http)
    {
        await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var signedOut = BaseCookie(http);
        signedOut.MaxAge = TimeSpan.FromDays(14);
        http.Response.Cookies.Append(DemoUsers.SignedOutCookie, "1", signedOut);
        return Results.LocalRedirect("/forums");
    }

    private static CookieOptions BaseCookie(HttpContext http) => new()
    {
        Path = "/",
        HttpOnly = true,
        IsEssential = true,
        SameSite = SameSiteMode.Lax,
        Secure = http.Request.IsHttps,
    };
}
