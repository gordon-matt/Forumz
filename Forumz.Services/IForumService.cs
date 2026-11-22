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

/// <summary>
/// Reads, writes and moderates the forum for the current tenant.
/// A caller who may not read is forbidden. Writing, subscribing, and the profile require a signed-in user.
/// A page number below 1 is read as 1.
/// </summary>
public interface IForumService
{
    /// <summary>Groups and forums for the index, in display order. A caller who may not read is forbidden.</summary>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The index, including whether the caller may manage it, or forbidden.</returns>
    Task<Result<ForumIndexModel>> GetIndexAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// One page of topics in a forum. Announcements come first, then sticky topics, then the latest reply.
    /// An unknown forum is not found.
    /// </summary>
    /// <param name="forumId">The forum.</param>
    /// <param name="pageNumber">The page, starting at 1.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The page, or not found or forbidden.</returns>
    Task<Result<ForumPageModel>> GetForumAsync(int forumId, int pageNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// One page of posts, oldest first. <paramref name="countView"/> increments the view counter.
    /// Each post includes sanitized HTML for display. An unknown topic is not found.
    /// </summary>
    /// <param name="topicId">The topic.</param>
    /// <param name="pageNumber">The page, starting at 1.</param>
    /// <param name="countView"><see langword="true"/> counts this open.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The topic page, or not found or forbidden.</returns>
    Task<Result<TopicPageModel>> GetTopicAsync(int topicId, int pageNumber, bool countView, CancellationToken cancellationToken = default);

    /// <summary>Topics with the newest replies, optionally inside one forum, ordered like a forum page. An unknown forum is not found.</summary>
    /// <param name="forumId">When set, only topics in this forum.</param>
    /// <param name="pageNumber">The page, starting at 1.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The page, or not found or forbidden.</returns>
    Task<Result<TopicListModel>> GetActiveAsync(int? forumId, int pageNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Topics whose subject or a post contains <paramref name="keywords"/>, ignoring case.
    /// A term shorter than the configured minimum is invalid. An unknown forum is not found.
    /// </summary>
    /// <param name="keywords">The search text. It is trimmed.</param>
    /// <param name="forumId">When set, only topics in this forum.</param>
    /// <param name="pageNumber">The page, starting at 1.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The matches, or invalid, not found, or forbidden.</returns>
    Task<Result<TopicListModel>> SearchAsync(string keywords, int? forumId, int pageNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a topic and its opening post. Returns the topic id.
    /// The subject and the post are required, and each must fit the current settings.
    /// A caller who cannot moderate always gets a normal topic. Subscribers other than the author are notified, and a notification failure leaves the topic in place.
    /// </summary>
    /// <param name="request">The topic. A null request is invalid.</param>
    /// <param name="ipAddress">The author's remote address. Longer than 45 characters is cut. <see langword="null"/> is stored as empty.</param>
    /// <param name="cancellationToken">Cancels the save.</param>
    /// <returns>The new topic id, or invalid, not found, or forbidden.</returns>
    Task<Result<int>> CreateTopicAsync(CreateTopicRequest request, string? ipAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a reply. Returns the post id. The text is required and must fit the current post length.
    /// Subscribers other than the author are notified, and a notification failure leaves the reply in place.
    /// </summary>
    /// <param name="request">The reply. A null request is invalid.</param>
    /// <param name="ipAddress">The author's remote address. Longer than 45 characters is cut. <see langword="null"/> is stored as empty.</param>
    /// <param name="cancellationToken">Cancels the save.</param>
    /// <returns>The new post id, or invalid, not found, or forbidden.</returns>
    Task<Result<int>> CreatePostAsync(CreatePostRequest request, string? ipAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces a post body. The author may edit it when the settings allow, and a moderator may edit any post.
    /// The text is required and must fit the current post length.
    /// </summary>
    /// <param name="request">The new body. A null request is invalid.</param>
    /// <param name="cancellationToken">Cancels the save.</param>
    /// <returns>Success, or invalid, not found, or forbidden.</returns>
    Task<Result> UpdatePostAsync(UpdatePostRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a post. Deleting the only post in a topic deletes the topic.
    /// The author's post count decreases, and the forum's counts are recalculated.
    /// </summary>
    /// <param name="postId">The post.</param>
    /// <param name="cancellationToken">Cancels the delete.</param>
    /// <returns>Success, or not found or forbidden.</returns>
    Task<Result> DeletePostAsync(int postId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a topic, its posts, and its topic subscriptions. The authors' post counts decrease.
    /// The author of the opening post may do this when members may delete their own posts. Otherwise a moderator is required.
    /// </summary>
    /// <param name="topicId">The topic.</param>
    /// <param name="cancellationToken">Cancels the delete.</param>
    /// <returns>Success, or not found or forbidden.</returns>
    Task<Result> DeleteTopicAsync(int topicId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves a topic to another forum in the same tenant. A moderator is required.
    /// Moving a topic onto the forum it is already in succeeds and changes nothing. Topic subscriptions follow the topic.
    /// </summary>
    /// <param name="topicId">The topic.</param>
    /// <param name="destinationForumId">The forum that should hold it.</param>
    /// <param name="cancellationToken">Cancels the move.</param>
    /// <returns>Success, or not found or forbidden.</returns>
    Task<Result> MoveTopicAsync(int topicId, int destinationForumId, CancellationToken cancellationToken = default);

    /// <summary>Pins, announces, or unpins a topic. A moderator is required. A value that is not a defined topic type is invalid.</summary>
    /// <param name="topicId">The topic.</param>
    /// <param name="topicType">Normal, sticky, or announcement.</param>
    /// <param name="cancellationToken">Cancels the change.</param>
    /// <returns>Success, or invalid, not found, or forbidden.</returns>
    Task<Result> SetTopicTypeAsync(int topicId, ForumTopicType topicType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a group. Returns the group id. A missing or zero id creates one.
    /// The name is required. A caller who may not manage the forum is forbidden.
    /// </summary>
    /// <param name="request">The group. A null request is invalid.</param>
    /// <param name="cancellationToken">Cancels the save.</param>
    /// <returns>The group id, or invalid, not found, or forbidden.</returns>
    Task<Result<int>> SaveGroupAsync(SaveForumGroupRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes an empty group. A group that still contains forums is invalid.</summary>
    /// <param name="groupId">The group.</param>
    /// <param name="cancellationToken">Cancels the delete.</param>
    /// <returns>Success, or invalid, not found, or forbidden.</returns>
    Task<Result> DeleteGroupAsync(int groupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a forum. Returns the forum id. A missing or zero id creates one.
    /// The name is required. The description may be empty. An update can move the forum to another group in the same tenant.
    /// </summary>
    /// <param name="request">The forum. A null request is invalid.</param>
    /// <param name="cancellationToken">Cancels the save.</param>
    /// <returns>The forum id, or invalid, not found, or forbidden.</returns>
    Task<Result<int>> SaveForumAsync(SaveForumRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a forum, its topics and posts, and the subscriptions that pointed at them. The authors' post counts decrease.</summary>
    /// <param name="forumId">The forum.</param>
    /// <param name="cancellationToken">Cancels the delete.</param>
    /// <returns>Success, or not found or forbidden.</returns>
    Task<Result> DeleteForumAsync(int forumId, CancellationToken cancellationToken = default);

    /// <summary>Groups and forums for the management page. A caller who may not manage the forum is forbidden.</summary>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The structure, or forbidden.</returns>
    Task<Result<ForumStructureModel>> GetStructureAsync(CancellationToken cancellationToken = default);

    /// <summary>Forums the move list can offer, in display order.</summary>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The forums, or forbidden.</returns>
    Task<Result<IReadOnlyList<ForumChoiceModel>>> GetForumChoicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Follows a whole forum. Subscribing again succeeds and adds nothing.
    /// Subscriptions that the settings have turned off are forbidden. An unknown forum is not found.
    /// </summary>
    /// <param name="forumId">The forum.</param>
    /// <param name="cancellationToken">Cancels the save.</param>
    /// <returns>Success, or not found or forbidden.</returns>
    Task<Result> SubscribeToForumAsync(int forumId, CancellationToken cancellationToken = default);

    /// <summary>Follows a topic. Subscribing again succeeds and adds nothing. An unknown topic is not found.</summary>
    /// <param name="topicId">The topic.</param>
    /// <param name="cancellationToken">Cancels the save.</param>
    /// <returns>Success, or not found or forbidden.</returns>
    Task<Result> SubscribeToTopicAsync(int topicId, CancellationToken cancellationToken = default);

    /// <summary>Removes one of the caller's subscriptions. Another member's subscription is reported as not found.</summary>
    /// <param name="subscriptionId">The subscription.</param>
    /// <param name="cancellationToken">Cancels the delete.</param>
    /// <returns>Success, or not found or forbidden.</returns>
    Task<Result> UnsubscribeAsync(int subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>The caller's subscriptions, newest first.</summary>
    /// <param name="pageNumber">The page, starting at 1.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The page, or forbidden when nobody is signed in.</returns>
    Task<Result<SubscriptionListModel>> GetSubscriptionsAsync(int pageNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Settings for the current tenant, creating the row from options the first time.
    /// Page sizes and lengths on that first row are clamped to the ranges a later save accepts.
    /// </summary>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The settings, or forbidden.</returns>
    Task<Result<ForumSettingsModel>> GetSettingsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces the settings row. Page sizes are clamped from 1 to 100. The search minimum is clamped from 1 to 20, and the feed size from 1 to 100.
    /// Subject and message-subject lengths stop at their columns. Post and message bodies stop at 100,000 characters.
    /// </summary>
    /// <param name="request">The settings. A null request is invalid.</param>
    /// <param name="cancellationToken">Cancels the save.</param>
    /// <returns>Success, or invalid or forbidden.</returns>
    Task<Result> SaveSettingsAsync(SaveForumSettingsRequest request, CancellationToken cancellationToken = default);

    /// <summary>The caller's forum profile, creating it on first read from the account name, or "Member" when the account has none.</summary>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The profile, or forbidden when nobody is signed in.</returns>
    Task<Result<MemberProfileModel>> GetProfileAsync(CancellationToken cancellationToken = default);

    /// <summary>Updates the caller's display name and signature. The name is required. The signature may be empty.</summary>
    /// <param name="request">The profile. A null request is invalid.</param>
    /// <param name="cancellationToken">Cancels the save.</param>
    /// <returns>Success, or invalid, not found, or forbidden.</returns>
    Task<Result> SaveProfileAsync(SaveProfileRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Newest topics for the RSS feed. <paramref name="forumId"/> limits it to one forum.
    /// Feeds that the settings have turned off are not found. Paths are <c>topic/{id}</c> with no leading slash.
    /// </summary>
    /// <param name="forumId">When set, only topics in this forum. An unknown forum is not found.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The feed, or not found or forbidden.</returns>
    Task<Result<FeedModel>> GetFeedAsync(int? forumId, CancellationToken cancellationToken = default);
}