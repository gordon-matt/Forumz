using Forumz.Components;
using Heroicsoft.NetForge.Abstractions.Auth;
using Heroicsoft.NetForge.Components;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Forumz;

/// <summary>Adds the Blazor forum to a host.</summary>
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the forum services and the Blazor circuit. The host still chooses a database
        /// with <c>AddForumzSqlite</c>, <c>AddForumzSqlServer</c>, <c>AddForumzNpgsql</c> or
        /// <c>AddForumzMySql</c>, then calls <c>app.MapForumz()</c>.
        /// Replace <see cref="IForumPresentation"/> to choose, per request, between the host layout and the forum's own top bar.
        /// </summary>
        /// <param name="configure">Mount path, title, and the home and logout links.</param>
        /// <returns>The same collection.</returns>
        public IServiceCollection AddForumz(Action<ForumzOptions>? configure = null)
        {
            services.AddForumzServices(configure);
            services.AddRazorComponents().AddInteractiveServerComponents();
            services.TryAddSingleton<IAuthProviderService>(new AuthProviderService(AuthProvider.Identity, null));
            services.AddNetForgeComponents();
            services.AddNetForgeMenuContributor<ForumzMenuContributor>();
            services.TryAddEnumerable(ServiceDescriptor.Scoped<CircuitHandler, ForumCircuitHandler>());
            services.TryAddScoped<ForumEditorCapabilities>();
            services.TryAddScoped<ForumUrls>();
            services.TryAddScoped<IForumPresentation, DefaultForumPresentation>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<MatcherPolicy, ForumPresentationPolicy>());
            return services;
        }
    }
}