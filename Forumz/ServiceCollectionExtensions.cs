using Heroicsoft.NetForge.Abstractions.Auth;
using Heroicsoft.NetForge.Components;
using Microsoft.AspNetCore.Components.Server.Circuits;
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
            return services;
        }
    }
}