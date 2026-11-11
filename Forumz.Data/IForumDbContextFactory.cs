namespace Forumz.Data;

/// <summary>
/// Forumz's own context factory. It extends Extenso's <see cref="IDbContextFactory"/> rather than
/// replacing the host's registration, and the repositories are built against this type explicitly.
/// </summary>
public interface IForumDbContextFactory : IDbContextFactory
{
    /// <summary>One of <see cref="Constants.DatabaseProviders"/>.</summary>
    string ProviderName { get; }
}