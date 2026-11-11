namespace Forumz.Services;

/// <summary>
/// Who may read and change the forum. The default lets anyone read, a signed-in user post, and an
/// administrator moderate. Replace it when the host has its own permissions.
/// </summary>
public interface IForumAccess
{
    /// <summary>Whether the caller may open the forum. Anonymous readers are allowed by the default.</summary>
    bool CanRead();

    /// <summary>Whether the caller may start topics and reply.</summary>
    bool CanWrite();

    /// <summary>Whether the caller may edit any post, move a topic, or change its type.</summary>
    bool CanModerate();

    /// <summary>Whether the caller may create forums and change settings.</summary>
    bool CanManage();
}

/// <summary>
/// The tenant for this request. Null means the host is not multi-tenant, and rows are stored with
/// an empty tenant id. A multi-tenant host returns the current tenant on every call, including
/// from inside the Blazor circuit, or tenants will share one forum.
/// </summary>
public interface IForumTenantScope
{
    /// <summary>The current tenant id, or null when the host has no tenants.</summary>
    string? TenantId { get; }
}

/// <summary>
/// Tells subscribers about a new post. The default only logs. A host replaces it to send mail.
/// A failure must not undo the post, and the default implementation does not throw.
/// </summary>
public interface IForumNotifier
{
    /// <summary>Called after the post has been stored.</summary>
    /// <param name="notice">The post and the subscribers, excluding the author.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    Task NotifyAsync(ForumPostNotice notice, CancellationToken cancellationToken = default);
}

/// <summary>A stored post and the people following it.</summary>
/// <param name="ForumId">Forum id.</param>
/// <param name="ForumName">Forum name.</param>
/// <param name="TopicId">Topic id.</param>
/// <param name="Subject">Topic subject.</param>
/// <param name="PostId">The new post.</param>
/// <param name="AuthorUserId">Author.</param>
/// <param name="AuthorName">Author's display name.</param>
/// <param name="SubscriberUserIds">Subscribers, not including the author.</param>
public sealed record ForumPostNotice(
    int ForumId,
    string ForumName,
    int TopicId,
    string Subject,
    int PostId,
    string AuthorUserId,
    string AuthorName,
    IReadOnlyList<string> SubscriberUserIds);

/// <summary>
/// The caller's remote address, captured when the circuit opens. A later button click has no
/// <c>HttpContext</c>, so the address would otherwise be lost.
/// </summary>
public sealed class ForumClientContext
{
    /// <summary>The remote address, or null when the circuit had none.</summary>
    public string? IpAddress { get; set; }
}

/// <summary>Reads, writes and moderates the forum for the current tenant.</summary>
public interface IForumService
{
    /// <summary>Groups and forums for the index.</summary>
    Task<Result<ForumIndexModel>> GetIndexAsync(CancellationToken cancellationToken = default);

    /// <summary>One page of topics in a forum.</summary>
    Task<Result<ForumPageModel>> GetForumAsync(int forumId, int pageNumber, CancellationToken cancellationToken = default);

    /// <summary>One page of posts. <paramref name="countView"/> increments the view counter.</summary>
    Task<Result<TopicPageModel>> GetTopicAsync(int topicId, int pageNumber, bool countView, CancellationToken cancellationToken = default);

    /// <summary>Topics with the newest replies, optionally inside one forum.</summary>
    Task<Result<TopicListModel>> GetActiveAsync(int? forumId, int pageNumber, CancellationToken cancellationToken = default);

    /// <summary>Topics whose subject or a post contains <paramref name="keywords"/>.</summary>
    Task<Result<TopicListModel>> SearchAsync(string keywords, int? forumId, int pageNumber, CancellationToken cancellationToken = default);

    /// <summary>Starts a topic and its opening post. Returns the topic id.</summary>
    Task<Result<int>> CreateTopicAsync(CreateTopicRequest request, string? ipAddress, CancellationToken cancellationToken = default);

    /// <summary>Adds a reply. Returns the post id.</summary>
    Task<Result<int>> CreatePostAsync(CreatePostRequest request, string? ipAddress, CancellationToken cancellationToken = default);

    /// <summary>Replaces a post body.</summary>
    Task<Result> UpdatePostAsync(UpdatePostRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a post. Deleting the opening post deletes the topic.</summary>
    Task<Result> DeletePostAsync(int postId, CancellationToken cancellationToken = default);

    /// <summary>Deletes a topic and its posts.</summary>
    Task<Result> DeleteTopicAsync(int topicId, CancellationToken cancellationToken = default);

    /// <summary>Moves a topic to another forum in the same tenant.</summary>
    Task<Result> MoveTopicAsync(int topicId, int destinationForumId, CancellationToken cancellationToken = default);

    /// <summary>Pins, announces, or unpins a topic.</summary>
    Task<Result> SetTopicTypeAsync(int topicId, ForumTopicType topicType, CancellationToken cancellationToken = default);

    /// <summary>Creates or updates a group. Returns the group id.</summary>
    Task<Result<int>> SaveGroupAsync(SaveForumGroupRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes an empty group.</summary>
    Task<Result> DeleteGroupAsync(int groupId, CancellationToken cancellationToken = default);

    /// <summary>Creates or updates a forum. Returns the forum id.</summary>
    Task<Result<int>> SaveForumAsync(SaveForumRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a forum and everything posted in it.</summary>
    Task<Result> DeleteForumAsync(int forumId, CancellationToken cancellationToken = default);

    /// <summary>Groups and forums for the management page.</summary>
    Task<Result<ForumStructureModel>> GetStructureAsync(CancellationToken cancellationToken = default);

    /// <summary>Forums the move list can offer.</summary>
    Task<Result<IReadOnlyList<ForumChoiceModel>>> GetForumChoicesAsync(CancellationToken cancellationToken = default);

    /// <summary>Follows a whole forum.</summary>
    Task<Result> SubscribeToForumAsync(int forumId, CancellationToken cancellationToken = default);

    /// <summary>Follows a topic.</summary>
    Task<Result> SubscribeToTopicAsync(int topicId, CancellationToken cancellationToken = default);

    /// <summary>Removes one of the caller's subscriptions.</summary>
    Task<Result> UnsubscribeAsync(int subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>The caller's subscriptions.</summary>
    Task<Result<SubscriptionListModel>> GetSubscriptionsAsync(int pageNumber, CancellationToken cancellationToken = default);

    /// <summary>Messages sent to the caller.</summary>
    Task<Result<MessageListModel>> GetInboxAsync(int pageNumber, CancellationToken cancellationToken = default);

    /// <summary>Messages the caller sent.</summary>
    Task<Result<MessageListModel>> GetSentAsync(int pageNumber, CancellationToken cancellationToken = default);

    /// <summary>Opens a message. Opening it as the recipient marks it read.</summary>
    Task<Result<PrivateMessageModel>> GetMessageAsync(int messageId, CancellationToken cancellationToken = default);

    /// <summary>Sends a private message. Returns the message id.</summary>
    Task<Result<int>> SendMessageAsync(SendMessageRequest request, CancellationToken cancellationToken = default);

    /// <summary>Hides a message from the caller. The row is removed when both sides have deleted it.</summary>
    Task<Result> DeleteMessageAsync(int messageId, CancellationToken cancellationToken = default);

    /// <summary>Unread inbox messages. Zero when the caller is signed out or messages are off.</summary>
    Task<Result<int>> GetUnreadMessageCountAsync(CancellationToken cancellationToken = default);

    /// <summary>Settings for the current tenant, creating the row from options the first time.</summary>
    Task<Result<ForumSettingsModel>> GetSettingsAsync(CancellationToken cancellationToken = default);

    /// <summary>Replaces the settings row.</summary>
    Task<Result> SaveSettingsAsync(SaveForumSettingsRequest request, CancellationToken cancellationToken = default);

    /// <summary>The caller's forum profile, creating it on first read.</summary>
    Task<Result<MemberProfileModel>> GetProfileAsync(CancellationToken cancellationToken = default);

    /// <summary>Updates the caller's display name and signature.</summary>
    Task<Result> SaveProfileAsync(SaveProfileRequest request, CancellationToken cancellationToken = default);

    /// <summary>Newest topics for the RSS feed. <paramref name="forumId"/> limits it to one forum.</summary>
    Task<Result<FeedModel>> GetFeedAsync(int? forumId, CancellationToken cancellationToken = default);
}
