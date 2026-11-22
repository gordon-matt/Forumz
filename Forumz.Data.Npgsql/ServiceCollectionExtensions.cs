using EasyChatz.Data.Npgsql;

namespace Forumz.Data.Npgsql;

/// <summary>Registers PostgreSQL as Forumz's store.</summary>
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Stores forums in PostgreSQL, in the <c>forumz</c> schema. EasyChatz uses the same database, in the <c>chatz</c> schema.
        /// Replaces a factory already registered, so calling a different provider afterwards switches storage.
        /// </summary>
        /// <param name="connectionString">PostgreSQL connection string.</param>
        /// <returns>The same collection.</returns>
        /// <exception cref="ArgumentException"><paramref name="connectionString"/> is null or white space.</exception>
        public IServiceCollection AddForumzNpgsql(string connectionString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
            services.RemoveAll<IForumDbContextFactory>();
            services.AddSingleton<IForumDbContextFactory>(new ForumzDbContextFactory(connectionString));
            services.AddEasyChatzNpgsql(connectionString);
            return services;
        }
    }
}