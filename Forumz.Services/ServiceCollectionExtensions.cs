using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Forumz.Services;

/// <summary>Registers the forum services. The host still adds one storage provider.</summary>
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the forum services and applies migrations at startup. Pair it with
        /// <c>AddForumzSqlite</c>, <c>AddForumzSqlServer</c>, <c>AddForumzNpgsql</c> or <c>AddForumzMySql</c>.
        /// A host that already registered <see cref="IUserContextService"/>, <see cref="IForumAccess"/>
        /// or <see cref="IForumTenantScope"/> keeps its own.
        /// </summary>
        /// <param name="configure">Mount path, title, and the settings used until an administrator saves their own.</param>
        /// <returns>The same collection.</returns>
        public IServiceCollection AddForumzServices(Action<ForumzOptions>? configure = null)
        {
            services.AddOptions<ForumzOptions>()
                .Configure(options => configure?.Invoke(options))
                .PostConfigure(options => options.BasePath = NormalizeBasePath(options.BasePath));

            services.AddHttpContextAccessor();
            services.TryAddSingleton(TimeProvider.System);
            services.TryAddScoped<ForumClientContext>();
            services.TryAddScoped<IUserContextService, ForumUserContext>();
            services.TryAddSingleton<IForumTenantScope, SingleTenantScope>();
            services.TryAddScoped<IForumAccess, DefaultForumAccess>();
            services.TryAddScoped<IForumNotifier, LoggingForumNotifier>();
            services.TryAddScoped<IForumContentRenderer, ForumContentRenderer>();

            services.TryAddScoped(Repository<ForumGroup>);
            services.TryAddScoped(Repository<Forum>);
            services.TryAddScoped(Repository<ForumTopic>);
            services.TryAddScoped(Repository<ForumPost>);
            services.TryAddScoped(Repository<ForumSubscription>);
            services.TryAddScoped(Repository<PrivateMessage>);
            services.TryAddScoped(Repository<ForumMember>);
            services.TryAddScoped(Repository<ForumPreference>);
            services.TryAddScoped<IForumService, ForumService>();
            services.AddHostedService<Hosting.ForumzDatabaseInitializer>();
            return services;
        }
    }

    private static IRepository<TEntity> Repository<TEntity>(IServiceProvider provider)
        where TEntity : class, IEntity =>
        new EntityFrameworkRepository<TEntity>(provider.GetRequiredService<IForumDbContextFactory>());

    private static string NormalizeBasePath(string? basePath)
    {
        string trimmed = (basePath ?? string.Empty).Trim().Trim('/');
        return trimmed.Length == 0 ? "/forums" : "/" + trimmed;
    }
}

/// <summary>
/// Reads the caller from the request when there is one, and from the Blazor circuit otherwise.
/// Registered only when the host has not already chosen a user context.
/// </summary>
internal sealed class ForumUserContext(IHttpContextAccessor httpContextAccessor, IServiceProvider services) : IUserContextService
{
    /// <inheritdoc />
    public string? GetCurrentUserId() => Principal().FindUserId();

    /// <inheritdoc />
    public string? GetCurrentUserName() => Principal()?.Identity?.Name;

    /// <inheritdoc />
    public bool IsAuthenticated() => Principal()?.Identity?.IsAuthenticated == true;

    /// <inheritdoc />
    public bool IsAdministrator() => Principal()?.IsInRole(WellKnownRoles.Administrator) == true;

    /// <inheritdoc />
    public IReadOnlyList<string> GetRoleNames()
    {
        var user = Principal();
        return user is null
            ? []
            : (IReadOnlyList<string>)user.Claims
            .Where(claim => claim.Type is ClaimTypes.Role or "role" or "roles")
            .Select(claim => claim.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private ClaimsPrincipal? Principal()
    {
        var httpUser = httpContextAccessor.HttpContext?.User;
        if (httpUser?.Identity?.IsAuthenticated == true)
        {
            return httpUser;
        }

        var provider = services.GetService<AuthenticationStateProvider>();
        if (provider is null)
        {
            return httpUser;
        }

        try
        {
            // The circuit's authentication state is already completed. IUserContextService is synchronous.
            var user = Task.Run(async () => (await provider.GetAuthenticationStateAsync().ConfigureAwait(false)).User)
                .GetAwaiter()
                .GetResult();
            return user.Identity?.IsAuthenticated == true ? user : httpUser;
        }
        catch (Exception ex) when (ex is InvalidOperationException or ObjectDisposedException)
        {
            return httpUser;
        }
    }
}