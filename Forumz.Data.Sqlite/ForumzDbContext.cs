using Microsoft.Data.Sqlite;

namespace Forumz.Data.Sqlite;

/// <summary>SQLite context. Schemas are not available, so tables are prefixed.</summary>
public class ForumzDbContext(DbContextOptions<ForumzDbContext> options) : ForumzDbContextBase(options)
{
    /// <inheritdoc />
    protected override bool SupportsSchemas => false;
}

/// <summary>Builds SQLite contexts for one connection string.</summary>
public class ForumzDbContextFactory(string connectionString) : IForumDbContextFactory
{
    private DbContextOptions<ForumzDbContext> Options => field ??= BuildOptions(connectionString);

    /// <inheritdoc />
    public string ProviderName => Constants.DatabaseProviders.Sqlite;

    /// <inheritdoc />
    public DbContext GetContext() => new ForumzDbContext(Options);

    /// <inheritdoc />
    public DbContext GetContext(string connectionString) => new ForumzDbContext(BuildOptions(connectionString));

    /// <summary>Options used by the app and by <c>dotnet ef</c>.</summary>
    /// <param name="connectionString">SQLite connection string.</param>
    /// <returns>The options.</returns>
    public static DbContextOptions<ForumzDbContext> BuildOptions(string connectionString)
    {
        var builder = new DbContextOptionsBuilder<ForumzDbContext>();
        // The loaded assembly, not its name. A plugin is not on the host's load path, so
        // Assembly.Load of the name cannot see these migrations and the tables are never created.
        builder.UseSqlite(Prepare(connectionString), sqlite => sqlite
            .MigrationsAssembly(typeof(ForumzDbContext).Assembly)
            .MigrationsHistoryTable(Constants.MigrationsHistoryTable));
        return builder.Options;
    }

    /// <summary>Creates the database folder and turns foreign keys on. SQLite creates a missing file, not a missing directory.</summary>
    private static string Prepare(string connectionString)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString) { ForeignKeys = true };

        if (builder.DataSource is { Length: > 0 } dataSource
            && !dataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase)
            && builder.Mode != SqliteOpenMode.Memory
            && Path.GetDirectoryName(Path.GetFullPath(dataSource)) is { Length: > 0 } directory)
        {
            Directory.CreateDirectory(directory);
        }

        return builder.ToString();
    }
}