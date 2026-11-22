using Microsoft.EntityFrameworkCore.Design;

namespace Forumz.Data.Sql;

/// <summary>Used by <c>dotnet ef</c> so migrations can be generated without a running SQL Server.</summary>
public class ForumzDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ForumzDbContext>
{
    /// <summary>A context aimed at a local database named <c>forumz_design</c>. The arguments are not used.</summary>
    /// <param name="args">The arguments <c>dotnet ef</c> passed. They are ignored.</param>
    /// <returns>The design-time context.</returns>
    public ForumzDbContext CreateDbContext(string[] args) =>
        new(ForumzDbContextFactory.BuildOptions(
            "Server=localhost;Database=forumz_design;Trusted_Connection=True;TrustServerCertificate=True"));
}