using Microsoft.EntityFrameworkCore.Design;

namespace Forumz.Data.Npgsql;

/// <summary>Used by <c>dotnet ef</c> so migrations can be generated without a running PostgreSQL server.</summary>
public class ForumzDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ForumzDbContext>
{
    /// <inheritdoc />
    public ForumzDbContext CreateDbContext(string[] args) =>
        new(ForumzDbContextFactory.BuildOptions(
            "Host=localhost;Database=forumz_design;Username=postgres;Password=postgres"));
}
