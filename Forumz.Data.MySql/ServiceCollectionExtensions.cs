using EasyChatz.Data.MySql;

namespace Forumz.Data.MySql;

/// <summary>Registers MySQL as Forumz's store.</summary>
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Stores forums in MySQL. Table names are prefixed because a MySQL schema is a database. EasyChatz uses the same database, with its own prefix.
        /// Replaces a factory already registered, so calling a different provider afterwards switches storage.
        /// </summary>
        /// <param name="connectionString">MySQL connection string.</param>
        /// <returns>The same collection.</returns>
        /// <exception cref="ArgumentException"><paramref name="connectionString"/> is null or white space.</exception>
        public IServiceCollection AddForumzMySql(string connectionString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
            services.RemoveAll<IForumDbContextFactory>();
            services.AddSingleton<IForumDbContextFactory>(new ForumzDbContextFactory(connectionString));
            services.AddEasyChatzMySql(connectionString);
            return services;
        }
    }
}