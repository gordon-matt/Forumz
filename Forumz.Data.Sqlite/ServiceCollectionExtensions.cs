using System.Reflection;
using EasyChatz.Data.Sqlite;

namespace Forumz.Data.Sqlite;

/// <summary>Registers SQLite as Forumz's store.</summary>
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Stores forums in SQLite, and EasyChatz in the same file. When <paramref name="connectionString"/>
        /// is omitted, the file is <c>Forumz.db</c> under the host's local application data, so a redeploy does not wipe it.
        /// Replaces a factory already registered, so calling a different provider afterwards switches storage.
        /// Tables are prefixed because SQLite has no schemas, and each connection turns foreign keys on.
        /// </summary>
        /// <param name="connectionString">A SQLite connection string, or null for the default file.</param>
        /// <returns>The same collection.</returns>
        public IServiceCollection AddForumzSqlite(string? connectionString = null)
        {
            string resolved = string.IsNullOrWhiteSpace(connectionString)
                ? $"Data Source={DefaultDatabasePath()}"
                : connectionString;

            services.RemoveAll<IForumDbContextFactory>();
            services.AddSingleton<IForumDbContextFactory>(new ForumzDbContextFactory(resolved));
            services.AddEasyChatzSqlite(resolved);
            return services;
        }
    }

    private static string DefaultDatabasePath()
    {
        string appName = Assembly.GetEntryAssembly()?.GetName().Name ?? "Forumz";
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName, "Forumz.db");
    }
}