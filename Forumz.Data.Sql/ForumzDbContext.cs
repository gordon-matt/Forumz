namespace Forumz.Data.Sql;

/// <summary>The SQL Server context. Tables live in the <c>forumz</c> schema, and migrations are stored in Forumz's own history table.</summary>
public class ForumzDbContext(DbContextOptions<ForumzDbContext> options) : ForumzDbContextBase(options)
{
}

/// <summary>Builds SQL Server contexts for one connection string.</summary>
public class ForumzDbContextFactory(string connectionString) : IForumDbContextFactory
{
    private DbContextOptions<ForumzDbContext> Options => field ??= BuildOptions(connectionString);

    /// <inheritdoc />
    public string ProviderName => Constants.DatabaseProviders.SqlServer;

    /// <inheritdoc />
    public DbContext GetContext() => new ForumzDbContext(Options);

    /// <inheritdoc />
    public DbContext GetContext(string connectionString) => new ForumzDbContext(BuildOptions(connectionString));

    /// <summary>Options used by the app and by <c>dotnet ef</c>.</summary>
    /// <param name="connectionString">SQL Server connection string.</param>
    /// <returns>Options for this connection string, including Forumz's migration history table.</returns>
    public static DbContextOptions<ForumzDbContext> BuildOptions(string connectionString)
    {
        var builder = new DbContextOptionsBuilder<ForumzDbContext>();
        // The loaded assembly, not its name. A plugin is not on the host's load path, so
        // Assembly.Load of the name cannot see these migrations and the tables are never created.
        builder.UseSqlServer(connectionString, sql => sql
            .MigrationsAssembly(typeof(ForumzDbContext).Assembly)
            .MigrationsHistoryTable(Constants.MigrationsHistoryTable, Constants.Schema));
        return builder.Options;
    }
}