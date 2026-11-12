namespace Forumz;

/// <summary>
/// Whether this request loads the forum's own document. The document has the forum's top bar.
/// When this is false and <see cref="ForumzOptions.PublicLayout"/> is set, the host's router draws the pages under its own bar.
/// </summary>
public interface IForumPresentation
{
    /// <summary>True to serve the forum document. False to leave the request to the host's router.</summary>
    bool UseOwnTopBar { get; }
}

/// <summary>
/// Uses <see cref="ForumzOptions.UseOwnTopBar"/>, and the forum's own document whenever no host layout is set.
/// </summary>
public sealed class DefaultForumPresentation(IOptions<ForumzOptions> options) : IForumPresentation
{
    /// <inheritdoc />
    public bool UseOwnTopBar => options.Value.PublicLayout is null || options.Value.UseOwnTopBar;
}
