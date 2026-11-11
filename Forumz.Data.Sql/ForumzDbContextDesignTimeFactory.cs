using Microsoft.EntityFrameworkCore.Design;

namespace Forumz.Data.Sql;

/// <summary>Used by <c>dotnet ef</c> so migrations can be generated without a running SQL Server.</summary>
public class ForumzDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ForumzDbContext>
{
    /// <inheritdoc />
    public ForumzDbContext CreateDbContext(string[] args) =>
        new(ForumzDbContextFactory.BuildOptions(
            "Server=localhost;Database=forumz_design;Trusted_Connection=True;TrustServerCertificate=True"));
}