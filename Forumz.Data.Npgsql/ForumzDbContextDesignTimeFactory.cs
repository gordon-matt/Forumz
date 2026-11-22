using Microsoft.EntityFrameworkCore.Design;

namespace Forumz.Data.Npgsql;

/// <summary>Used by <c>dotnet ef</c> so migrations can be generated without a running PostgreSQL server.</summary>
public class ForumzDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ForumzDbContext>
{
    /// <summary>A context aimed at a local database named <c>forumz_design</c>. The arguments are not used.</summary>
    /// <param name="args">The arguments <c>dotnet ef</c> passed. They are ignored.</param>
    /// <returns>The design-time context.</returns>
    public ForumzDbContext CreateDbContext(string[] args) =>
        new(ForumzDbContextFactory.BuildOptions(
            "Host=localhost;Database=forumz_design;Username=postgres;Password=postgres"));
}