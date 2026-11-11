namespace Forumz.Data.Npgsql;

/// <summary>Registers PostgreSQL as Forumz's store.</summary>
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>Stores forums in PostgreSQL, in the <c>forumz</c> schema.</summary>
        /// <param name="connectionString">PostgreSQL connection string.</param>
        /// <returns>The same collection.</returns>
        public IServiceCollection AddForumzNpgsql(string connectionString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
            services.RemoveAll<IForumDbContextFactory>();
            services.AddSingleton<IForumDbContextFactory>(new ForumzDbContextFactory(connectionString));
            return services;
        }
    }
}
