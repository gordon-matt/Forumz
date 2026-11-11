using System.Reflection;

namespace Forumz.Data.Sqlite;

/// <summary>Registers SQLite as Forumz's store.</summary>
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Stores forums in SQLite. When <paramref name="connectionString"/> is omitted, the file
        /// is <c>Forumz.db</c> under the host's local application data, so a redeploy does not wipe it.
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
            return services;
        }
    }

    private static string DefaultDatabasePath()
    {
        string appName = Assembly.GetEntryAssembly()?.GetName().Name ?? "Forumz";
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName, "Forumz.db");
    }
}