namespace Forumz.Data.Sql;

/// <summary>Registers SQL Server as Forumz's store.</summary>
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>Stores forums in SQL Server, in the <c>forumz</c> schema.</summary>
        /// <param name="connectionString">SQL Server connection string.</param>
        /// <returns>The same collection.</returns>
        public IServiceCollection AddForumzSqlServer(string connectionString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
            services.RemoveAll<IForumDbContextFactory>();
            services.AddSingleton<IForumDbContextFactory>(new ForumzDbContextFactory(connectionString));
            return services;
        }
    }
}
