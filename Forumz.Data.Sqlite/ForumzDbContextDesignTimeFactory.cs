using Microsoft.EntityFrameworkCore.Design;

namespace Forumz.Data.Sqlite;

/// <summary>Used by <c>dotnet ef</c> so migrations can be generated without a host.</summary>
public class ForumzDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ForumzDbContext>
{
    /// <summary>A context aimed at <c>forumz_design.db</c> in the working directory. The arguments are not used.</summary>
    /// <param name="args">The arguments <c>dotnet ef</c> passed. They are ignored.</param>
    /// <returns>The design-time context.</returns>
    public ForumzDbContext CreateDbContext(string[] args) =>
        new(ForumzDbContextFactory.BuildOptions("Data Source=forumz_design.db"));
}