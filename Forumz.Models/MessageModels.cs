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