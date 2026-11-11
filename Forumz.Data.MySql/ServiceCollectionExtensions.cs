namespace Forumz.Data.MySql;

/// <summary>Registers MySQL as Forumz's store.</summary>
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>Stores forums in MySQL. Table names are prefixed because a MySQL schema is a database.</summary>
        /// <param name="connectionString">MySQL connection string.</param>
        /// <returns>The same collection.</returns>
        public IServiceCollection AddForumzMySql(string connectionString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
            services.RemoveAll<IForumDbContextFactory>();
            services.AddSingleton<IForumDbContextFactory>(new ForumzDbContextFactory(connectionString));
            return services;
        }
    }
}