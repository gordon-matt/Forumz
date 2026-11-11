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
    /// <returns>The options.</returns>
    public static DbContextOptions<ForumzDbContext> BuildOptions(string connectionString)
    {
        var builder = new DbContextOptionsBuilder<ForumzDbContext>();
        builder.UseMySQL(connectionString, mySql => mySql
            .MigrationsAssembly(typeof(ForumzDbContext).Assembly.GetName().Name)
            .MigrationsHistoryTable(Constants.MigrationsHistoryTable));
        return builder.Options;
    }
}
