namespace Forumz.Models;

/// <summary>The forum index: groups, and the forums inside each group.</summary>
/// <param name="Groups">Groups in display order. Empty when nothing has been created.</param>
/// <param name="CanManage">Whether the caller may open the structure editor.</param>
public sealed record ForumIndexModel(IReadOnlyList<ForumGroupModel> Groups, bool CanManage);

/// <summary>A group heading on the index.</summary>
/// <param name="Id">Group id.</param>
/// <param name="Name">Group name.</param>
/// <param name="Forums">Forums in display order.</param>
public sealed record ForumGroupModel(int Id, string Name, IReadOnlyList<ForumRowModel> Forums);

/// <summary>One forum on the index, including the last reply.</summary>
/// <param name="Id">Forum id.</param>
/// <param name="Name">Forum name.</param>
/// <param name="Description">Forum description. Empty when none was set.</param>
/// <param name="TopicCount">Topics in the forum.</param>
/// <param name="PostCount">Posts in the forum, including the first post of each topic.</param>
/// <param name="LastTopicId">Topic of the latest post, or null when the forum is empty.</param>
/// <param name="LastAuthorName">Display name of whoever wrote that post.</param>
/// <param name="LastPostTime">When that post was written, in UTC.</param>
public sealed record ForumRowModel(
    int Id,
    string Name,
    string Description,
    int TopicCount,
    int PostCount,
    int? LastTopicId,
    string? LastAuthorName,
    DateTime? LastPostTime);

/// <summary>A forum page of topics.</summary>
/// <param name="Id">Forum id.</param>
/// <param name="GroupId">Owning group.</param>
/// <param name="GroupName">Owning group name, for the breadcrumb.</param>
/// <param name="Name">Forum name.</param>
/// <param name="Description">Forum description.</param>
/// <param name="CanStartTopic">Whether the caller may start a topic here.</param>
/// <param name="CanSubscribe">Whether the caller may subscribe.</param>
/// <param name="IsSubscribed">Whether the caller already subscribes to the whole forum.</param>
/// <param name="SubscriptionId">The caller's forum subscription, when there is one.</param>
/// <param name="Topics">The page of topics.</param>
/// <param name="PageNumber">1-based page.</param>
/// <param name="PageSize">Page size from settings.</param>
/// <param name="TotalCount">Topics in the forum.</param>
public sealed record ForumPageModel(
    int Id,
    int GroupId,
    string GroupName,
    string Name,
    string Description,
    bool CanStartTopic,
    bool CanSubscribe,
    bool IsSubscribed,
    int? SubscriptionId,
    IReadOnlyList<TopicRowModel> Topics,
    int PageNumber,
    int PageSize,
    int TotalCount);

/// <summary>A topic row on a forum, search, or active list.</summary>
/// <param name="Id">Topic id.</param>
/// <param name="ForumId">Forum the topic belongs to.</param>
/// <param name="ForumName">Forum name, shown on lists that mix forums.</param>
/// <param name="Subject">Subject line.</param>
/// <param name="TopicType">Normal, sticky, or announcement.</param>
/// <param name="ReplyCount">Posts after the first.</param>
/// <param name="Views">Times the topic has been opened.</param>
/// <param name="AuthorName">Who started it.</param>
/// <param name="LastAuthorName">Who replied last.</param>
/// <param name="LastPostTime">When the latest post was written, in UTC.</param>
public sealed record TopicRowModel(
    int Id,
    int ForumId,
    string ForumName,
    string Subject,
    ForumTopicType TopicType,
    int ReplyCount,
    int Views,
    string AuthorName,
    string? LastAuthorName,
    DateTime? LastPostTime);

/// <summary>A paged list of topics that is not one forum, such as search or active discussions.</summary>
/// <param name="Title">Heading for the page.</param>
/// <param name="Topics">The page of topics.</param>
/// <param name="PageNumber">1-based page.</param>
/// <param name="PageSize">Page size from settings.</param>
/// <param name="TotalCount">Matching topics.</param>
public sealed record TopicListModel(
    string Title,
    IReadOnlyList<TopicRowModel> Topics,
    int PageNumber,
    int PageSize,
    int TotalCount);

/// <summary>A topic and one page of its posts.</summary>
/// <param name="Id">Topic id.</param>
/// <param name="ForumId">Forum id.</param>
/// <param name="ForumName">Forum name.</param>
/// <param name="GroupId">Group id.</param>
/// <param name="GroupName">Group name.</param>
/// <param name="Subject">Subject line.</param>
/// <param name="TopicType">Normal, sticky, or announcement.</param>
/// <param name="Views">Times the topic has been opened, including this view when it was counted.</param>
/// <param name="CanReply">Whether the caller may reply.</param>
/// <param name="CanModerate">Whether the caller may move, pin, or remove the topic.</param>
/// <param name="CanSubscribe">Whether the caller may subscribe.</param>
/// <param name="IsSubscribed">Whether the caller subscribes to this topic.</param>
/// <param name="SubscriptionId">The caller's topic subscription, when there is one.</param>
/// <param name="Posts">The page of posts, oldest first.</param>
/// <param name="PageNumber">1-based page.</param>
/// <param name="PageSize">Page size from settings.</param>
/// <param name="TotalCount">Posts in the topic.</param>
public sealed record TopicPageModel(
    int Id,
    int ForumId,
    string ForumName,
    int GroupId,
    string GroupName,
    string Subject,
    ForumTopicType TopicType,
    int Views,
    bool CanReply,
    bool CanModerate,
    bool CanSubscribe,
    bool IsSubscribed,
    int? SubscriptionId,
    IReadOnlyList<PostModel> Posts,
    int PageNumber,
    int PageSize,
    int TotalCount);

/// <summary>One post. <see cref="Text"/> is plain text; the page encodes it.</summary>
/// <param name="Id">Post id.</param>
/// <param name="AuthorUserId">Host user id of the author.</param>
/// <param name="AuthorName">Name shown on the post.</param>
/// <param name="Text">Plain text body.</param>
/// <param name="Signature">Author signature when signatures are enabled.</param>
/// <param name="CreatedOnUtc">When the post was written.</param>
/// <param name="UpdatedOnUtc">When the post was last edited. Equal to <paramref name="CreatedOnUtc"/> until then.</param>
/// <param name="CanEdit">Whether the caller may edit it.</param>
/// <param name="CanDelete">Whether the caller may delete it.</param>
public sealed record PostModel(
    int Id,
    string AuthorUserId,
    string AuthorName,
    string Text,
    string? Signature,
    DateTime CreatedOnUtc,
    DateTime UpdatedOnUtc,
    bool CanEdit,
    bool CanDelete);
