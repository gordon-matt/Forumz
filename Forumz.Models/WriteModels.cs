namespace Forumz.Models;

/// <summary>Starts a topic. The first post is <see cref="Text"/>.</summary>
/// <param name="ForumId">Forum that will hold the topic.</param>
/// <param name="Subject">Subject line.</param>
/// <param name="Text">The opening post, as Markdown or HTML according to <paramref name="Format"/>.</param>
/// <param name="TopicType">Ignored unless the caller can moderate. Everyone else gets a normal topic.</param>
/// <param name="Format">Markdown unless the author used the rich text editor.</param>
public sealed record CreateTopicRequest(int ForumId, string Subject, string Text, ForumTopicType TopicType = ForumTopicType.Normal, ContentFormat Format = ContentFormat.Markdown);

/// <summary>Adds a reply.</summary>
/// <param name="TopicId">Topic to reply to.</param>
/// <param name="Text">Markdown or HTML, according to <paramref name="Format"/>.</param>
/// <param name="Format">Markdown unless the author used the rich text editor.</param>
public sealed record CreatePostRequest(int TopicId, string Text, ContentFormat Format = ContentFormat.Markdown);

/// <summary>Replaces a post body.</summary>
/// <param name="PostId">Post to change.</param>
/// <param name="Text">Markdown or HTML, according to <paramref name="Format"/>.</param>
/// <param name="Format">Markdown unless the author used the rich text editor.</param>
public sealed record UpdatePostRequest(int PostId, string Text, ContentFormat Format = ContentFormat.Markdown);

/// <summary>Creates or renames a group. A missing or zero <see cref="Id"/> creates one.</summary>
/// <param name="Id">Existing group, or null to create.</param>
/// <param name="Name">Group name.</param>
/// <param name="DisplayOrder">Lower numbers are listed first.</param>
public sealed record SaveForumGroupRequest(int? Id, string Name, int DisplayOrder);

/// <summary>Creates or updates a forum. A missing or zero <see cref="Id"/> creates one.</summary>
/// <param name="Id">Existing forum, or null to create.</param>
/// <param name="GroupId">Group that owns the forum.</param>
/// <param name="Name">Forum name.</param>
/// <param name="Description">Shown under the name. Empty is allowed.</param>
/// <param name="DisplayOrder">Lower numbers are listed first inside the group.</param>
public sealed record SaveForumRequest(int? Id, int GroupId, string Name, string Description, int DisplayOrder);

/// <summary>Groups and forums for the management page.</summary>
/// <param name="Groups">Every group in the current tenant.</param>
public sealed record ForumStructureModel(IReadOnlyList<ForumGroupEditModel> Groups);

/// <summary>A group on the management page.</summary>
/// <param name="Id">Group id.</param>
/// <param name="Name">Group name.</param>
/// <param name="DisplayOrder">Sort key.</param>
/// <param name="Forums">Forums in the group.</param>
public sealed record ForumGroupEditModel(int Id, string Name, int DisplayOrder, IReadOnlyList<ForumEditModel> Forums);

/// <summary>A forum on the management page.</summary>
/// <param name="Id">Forum id.</param>
/// <param name="GroupId">Owning group.</param>
/// <param name="Name">Forum name.</param>
/// <param name="Description">Forum description.</param>
/// <param name="DisplayOrder">Sort key inside the group.</param>
public sealed record ForumEditModel(int Id, int GroupId, string Name, string Description, int DisplayOrder);

/// <summary>A forum the move-topic list can offer.</summary>
/// <param name="Id">Forum id.</param>
/// <param name="GroupName">Group name, so two forums with the same name stay distinct.</param>
/// <param name="Name">Forum name.</param>
public sealed record ForumChoiceModel(int Id, string GroupName, string Name);