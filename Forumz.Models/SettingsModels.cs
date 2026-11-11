namespace Forumz.Models;

/// <summary>The settings an administrator can change. Lengths are already clamped to the columns.</summary>
/// <param name="AllowUsersToEditPosts">Members may edit their own posts.</param>
/// <param name="AllowUsersToDeletePosts">Members may delete their own posts.</param>
/// <param name="AllowUsersToManageSubscriptions">Members may subscribe.</param>
/// <param name="AllowPrivateMessages">Private messages are available.</param>
/// <param name="RelativeDates">Show relative times.</param>
/// <param name="FeedsEnabled">Publish RSS feeds.</param>
/// <param name="SignaturesEnabled">Show signatures under posts.</param>
/// <param name="TopicsPageSize">Topics per forum page.</param>
/// <param name="PostsPageSize">Posts per topic page.</param>
/// <param name="SearchPageSize">Search results per page.</param>
/// <param name="ActivePageSize">Active topics per page.</param>
/// <param name="MessagesPageSize">Messages per page.</param>
/// <param name="SubscriptionsPageSize">Subscriptions per page.</param>
/// <param name="SubjectMaxLength">Longest topic subject.</param>
/// <param name="PostMaxLength">Longest post.</param>
/// <param name="MessageSubjectMaxLength">Longest private-message subject.</param>
/// <param name="MessageTextMaxLength">Longest private-message body.</param>
/// <param name="SearchMinimumLength">Shortest search that is accepted.</param>
/// <param name="FeedCount">Items in a feed.</param>
public sealed record ForumSettingsModel(
    bool AllowUsersToEditPosts,
    bool AllowUsersToDeletePosts,
    bool AllowUsersToManageSubscriptions,
    bool AllowPrivateMessages,
    bool RelativeDates,
    bool FeedsEnabled,
    bool SignaturesEnabled,
    int TopicsPageSize,
    int PostsPageSize,
    int SearchPageSize,
    int ActivePageSize,
    int MessagesPageSize,
    int SubscriptionsPageSize,
    int SubjectMaxLength,
    int PostMaxLength,
    int MessageSubjectMaxLength,
    int MessageTextMaxLength,
    int SearchMinimumLength,
    int FeedCount);

/// <summary>Replaces the settings row. The same fields as <see cref="ForumSettingsModel"/>.</summary>
/// <param name="AllowUsersToEditPosts">Members may edit their own posts.</param>
/// <param name="AllowUsersToDeletePosts">Members may delete their own posts.</param>
/// <param name="AllowUsersToManageSubscriptions">Members may subscribe.</param>
/// <param name="AllowPrivateMessages">Private messages are available.</param>
/// <param name="RelativeDates">Show relative times.</param>
/// <param name="FeedsEnabled">Publish RSS feeds.</param>
/// <param name="SignaturesEnabled">Show signatures under posts.</param>
/// <param name="TopicsPageSize">Topics per forum page.</param>
/// <param name="PostsPageSize">Posts per topic page.</param>
/// <param name="SearchPageSize">Search results per page.</param>
/// <param name="ActivePageSize">Active topics per page.</param>
/// <param name="MessagesPageSize">Messages per page.</param>
/// <param name="SubscriptionsPageSize">Subscriptions per page.</param>
/// <param name="SubjectMaxLength">Longest topic subject.</param>
/// <param name="PostMaxLength">Longest post.</param>
/// <param name="MessageSubjectMaxLength">Longest private-message subject.</param>
/// <param name="MessageTextMaxLength">Longest private-message body.</param>
/// <param name="SearchMinimumLength">Shortest search that is accepted.</param>
/// <param name="FeedCount">Items in a feed.</param>
public sealed record SaveForumSettingsRequest(
    bool AllowUsersToEditPosts,
    bool AllowUsersToDeletePosts,
    bool AllowUsersToManageSubscriptions,
    bool AllowPrivateMessages,
    bool RelativeDates,
    bool FeedsEnabled,
    bool SignaturesEnabled,
    int TopicsPageSize,
    int PostsPageSize,
    int SearchPageSize,
    int ActivePageSize,
    int MessagesPageSize,
    int SubscriptionsPageSize,
    int SubjectMaxLength,
    int PostMaxLength,
    int MessageSubjectMaxLength,
    int MessageTextMaxLength,
    int SearchMinimumLength,
    int FeedCount);

/// <summary>The signed-in member's forum profile.</summary>
/// <param name="DisplayName">Name shown on posts.</param>
/// <param name="Signature">Shown under posts when signatures are enabled.</param>
/// <param name="PostCount">Posts the member has written.</param>
public sealed record MemberProfileModel(string DisplayName, string Signature, int PostCount);

/// <summary>Updates the signed-in member's name and signature.</summary>
/// <param name="DisplayName">Name shown on posts. Required.</param>
/// <param name="Signature">Optional. Plain text.</param>
public sealed record SaveProfileRequest(string DisplayName, string Signature);

/// <summary>Items for an RSS feed. Links are paths under the forum's base path, such as <c>topic/4</c>.</summary>
/// <param name="Title">Feed title.</param>
/// <param name="Description">Feed description.</param>
/// <param name="Items">Newest first.</param>
public sealed record FeedModel(string Title, string Description, IReadOnlyList<FeedItemModel> Items);

/// <summary>One RSS item.</summary>
/// <param name="Title">Item title.</param>
/// <param name="Path">Path under the forum base, with no leading slash.</param>
/// <param name="Summary">Plain text excerpt.</param>
/// <param name="PublishedOnUtc">When the post was written.</param>
public sealed record FeedItemModel(string Title, string Path, string Summary, DateTime PublishedOnUtc);
