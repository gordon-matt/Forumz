namespace Forumz.Data.MySql;

/// <summary>MySQL context. A schema is a database, so tables are prefixed instead.</summary>
public class ForumzDbContext(DbContextOptions<ForumzDbContext> options) : ForumzDbContextBase(options)
{
    /// <inheritdoc />
    protected override bool SupportsSchemas => false;
}

/// <summary>Builds MySQL contexts for one connection string.</summary>
public class ForumzDbContextFactory(string connectionString) : IForumDbContextFactory
{
    private DbContextOptions<ForumzDbContext> Options => field ??= BuildOptions(connectionString);

    /// <inheritdoc />
    public string ProviderName => Constants.DatabaseProviders.MySql;

    /// <inheritdoc />
    public DbContext GetContext() => new ForumzDbContext(Options);

    /// <inheritdoc />
    public DbContext GetContext(string connectionString) => new ForumzDbContext(BuildOptions(connectionString));

    /// <summary>Options used by the app and by <c>dotnet ef</c>.</summary>
    /// <param name="connectionString">MySQL connection string.</param>
    /// <returns>Options for this connection string, including Forumz's migration history table.</returns>
    public static DbContextOptions<ForumzDbContext> BuildOptions(string connectionString)
    {
        var builder = new DbContextOptionsBuilder<ForumzDbContext>();
        // The loaded assembly, not its name. A plugin is not on the host's load path, so
        // Assembly.Load of the name cannot see these migrations and the tables are never created.
        builder.UseMySQL(connectionString, mySql => mySql
            .MigrationsAssembly(typeof(ForumzDbContext).Assembly)
            .MigrationsHistoryTable(Constants.MigrationsHistoryTable));
        return builder.Options;
    }
}