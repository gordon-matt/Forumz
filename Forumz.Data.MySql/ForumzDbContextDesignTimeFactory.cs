using Microsoft.EntityFrameworkCore.Design;

namespace Forumz.Data.MySql;

/// <summary>Used by <c>dotnet ef</c> so migrations can be generated without a running MySQL server.</summary>
public class ForumzDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ForumzDbContext>
{
    /// <inheritdoc />
    public ForumzDbContext CreateDbContext(string[] args) =>
        new(ForumzDbContextFactory.BuildOptions("Server=localhost;Database=forumz_design;User=root;Password=root"));
}
