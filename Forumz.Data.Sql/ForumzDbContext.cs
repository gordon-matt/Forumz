namespace Forumz.Data.Sql;

/// <summary>SQL Server context.</summary>
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
    /// <returns>The options.</returns>
    public static DbContextOptions<ForumzDbContext> BuildOptions(string connectionString)
    {
        var builder = new DbContextOptionsBuilder<ForumzDbContext>();
        builder.UseSqlServer(connectionString, sql => sql
            .MigrationsAssembly(typeof(ForumzDbContext).Assembly.GetName().Name)
            .MigrationsHistoryTable(Constants.MigrationsHistoryTable, Constants.Schema));
        return builder.Options;
    }
}
