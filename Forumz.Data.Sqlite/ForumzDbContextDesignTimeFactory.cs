using Microsoft.EntityFrameworkCore.Design;

namespace Forumz.Data.Sqlite;

/// <summary>Used by <c>dotnet ef</c> so migrations can be generated without a host.</summary>
public class ForumzDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ForumzDbContext>
{
    /// <inheritdoc />
    public ForumzDbContext CreateDbContext(string[] args) =>
        new(ForumzDbContextFactory.BuildOptions("Data Source=forumz_design.db"));
}