namespace Forumz.Models;

/// <summary>A page of the caller's subscriptions.</summary>
/// <param name="Items">The page.</param>
/// <param name="PageNumber">1-based page.</param>
/// <param name="PageSize">Page size from settings.</param>
/// <param name="TotalCount">Subscriptions the caller has.</param>
public sealed record SubscriptionListModel(
    IReadOnlyList<SubscriptionModel> Items,
    int PageNumber,
    int PageSize,
    int TotalCount);

/// <summary>One subscription. <see cref="TopicId"/> is null when the subscription is for a whole forum.</summary>
/// <param name="Id">Subscription id, passed back to unsubscribe.</param>
/// <param name="ForumId">Forum id.</param>
/// <param name="ForumName">Forum name.</param>
/// <param name="TopicId">Topic id, or null for a forum subscription.</param>
/// <param name="TopicSubject">Topic subject, or null for a forum subscription.</param>
/// <param name="CreatedOnUtc">When the caller subscribed.</param>
public sealed record SubscriptionModel(
    int Id,
    int ForumId,
    string ForumName,
    int? TopicId,
    string? TopicSubject,
    DateTime CreatedOnUtc);

/// <summary>A page of private messages.</summary>
/// <param name="Items">The page.</param>
/// <param name="PageNumber">1-based page.</param>
/// <param name="PageSize">Page size from settings.</param>
/// <param name="TotalCount">Messages in this folder.</param>
/// <param name="UnreadCount">Unread messages in the inbox, including those on other pages.</param>
public sealed record MessageListModel(
    IReadOnlyList<MessageRowModel> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int UnreadCount);

/// <summary>A private message in the inbox or sent list.</summary>
/// <param name="Id">Message id.</param>
/// <param name="OtherUserId">The other person: the sender in the inbox, the recipient in sent.</param>
/// <param name="OtherName">That person's display name.</param>
/// <param name="Subject">Subject line.</param>
/// <param name="IsRead">Whether the recipient has opened it.</param>
/// <param name="CreatedOnUtc">When it was sent.</param>
public sealed record MessageRowModel(
    int Id,
    string OtherUserId,
    string OtherName,
    string Subject,
    bool IsRead,
    DateTime CreatedOnUtc);

/// <summary>A private message opened by the sender or the recipient.</summary>
/// <param name="Id">Message id.</param>
/// <param name="FromUserId">Sender.</param>
/// <param name="FromName">Sender's display name.</param>
/// <param name="ToUserId">Recipient.</param>
/// <param name="ToName">Recipient's display name.</param>
/// <param name="Subject">Subject line.</param>
/// <param name="Text">Plain text body.</param>
/// <param name="IsRead">Whether the recipient has opened it.</param>
/// <param name="CreatedOnUtc">When it was sent.</param>
/// <param name="IsInbox">True when the caller is the recipient.</param>
public sealed record PrivateMessageModel(
    int Id,
    string FromUserId,
    string FromName,
    string ToUserId,
    string ToName,
    string Subject,
    string Text,
    bool IsRead,
    DateTime CreatedOnUtc,
    bool IsInbox);

/// <summary>
/// Sends a private message. <see cref="To"/> is a display name or a user id. Names are matched
/// within the current tenant, and only members who already have a forum profile can be reached.
/// </summary>
/// <param name="To">Display name or user id of the recipient.</param>
/// <param name="Subject">Subject line.</param>
/// <param name="Text">Plain text body.</param>
public sealed record SendMessageRequest(string To, string Subject, string Text);