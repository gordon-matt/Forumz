namespace Forumz.Services;

/// <summary>The default access rules: public reading, signed-in posting, administrator moderation.</summary>
public sealed class DefaultForumAccess(IUserContextService users) : IForumAccess
{
    /// <inheritdoc />
    public bool CanRead() => true;

    /// <inheritdoc />
    public bool CanWrite() => users.IsAuthenticated() && !string.IsNullOrWhiteSpace(users.GetCurrentUserId());

    /// <inheritdoc />
    public bool CanModerate() => users.IsAdministrator();

    /// <inheritdoc />
    public bool CanManage() => users.IsAdministrator();
}

/// <summary>A host with no tenants. Every row is stored with an empty tenant id.</summary>
public sealed class SingleTenantScope : IForumTenantScope
{
    /// <inheritdoc />
    public string? TenantId => null;
}

/// <summary>Logs that a post was created. Mail is a host concern, so nothing is sent.</summary>
public sealed class LoggingForumNotifier(ILogger<LoggingForumNotifier> logger) : IForumNotifier
{
    /// <inheritdoc />
    public Task NotifyAsync(ForumPostNotice notice, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Topic {TopicId} has a new post {PostId} for {SubscriberCount} subscribers. Replace IForumNotifier to send mail.",
            notice.TopicId,
            notice.PostId,
            notice.SubscriberUserIds.Count);
        return Task.CompletedTask;
    }
}