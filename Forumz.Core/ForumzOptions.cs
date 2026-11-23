namespace Forumz.Core;

/// <summary>
/// How the forum is mounted, and the settings written the first time a tenant opens it.
/// Page sizes and lengths after that come from the settings row, which an administrator can change.
/// </summary>
public sealed class ForumzOptions
{
    /// <summary>Configuration section bound by the sample host.</summary>
    public const string SectionName = "Forumz";

    /// <summary>Route prefix. The host calls <c>MapForumz()</c> and this is where the circuit is mounted.</summary>
    public string BasePath { get; set; } = "/forums";

    /// <summary>
    /// The host site's own layout. When set, the forum can be drawn inside that layout and its bar
    /// becomes the submenu under the host bar. Leave this null and the forum stays its own document.
    /// </summary>
    public Type? PublicLayout { get; set; }

    /// <summary>
    /// Serve the forum's own document and top bar even when <see cref="PublicLayout"/> is set.
    /// A host that decides per request, such as one tenant at a time, replaces <c>IForumPresentation</c> instead.
    /// </summary>
    public bool UseOwnTopBar { get; set; }

    /// <summary>Browser title and the name in the shell when the host has not set a brand.</summary>
    public string Title { get; set; } = "Forums";

    /// <summary>
    /// Link back to the host site. Empty hides it. A relative path is rooted so the forum's
    /// <c>&lt;base href&gt;</c> does not keep the link inside the forum.
    /// </summary>
    public string HomePath { get; set; } = "/";

    /// <summary>
    /// Identity logout URL. Empty hides sign-out. The forum posts a normal form there because
    /// Blazor would swallow a link that only looks like navigation.
    /// </summary>
    public string LogoutPath { get; set; } = string.Empty;

    /// <summary>
    /// Extra Razor component at the end of the forum top bar, before the account link.
    /// The sample sets this to its user switcher. Null shows nothing. The component is created with no parameters.
    /// </summary>
    public Type? TopbarComponent { get; set; }

    /// <summary>Shared-menu name when the host draws <c>UseSharedMenu</c> inside the forum shell.</summary>
    public string MenuName { get; set; } = "Forumz";

    /// <summary>Apply migrations at startup. Turn this off when the host runs migrations itself.</summary>
    public bool AutoMigrate { get; set; } = true;

    /// <summary>Members may edit their own posts. Moderators can always edit.</summary>
    public bool AllowUsersToEditPosts { get; set; } = true;

    /// <summary>Members may delete their own posts. Moderators can always delete.</summary>
    public bool AllowUsersToDeletePosts { get; set; } = true;

    /// <summary>Members may subscribe and leave. Moderators can always subscribe.</summary>
    public bool AllowUsersToManageSubscriptions { get; set; } = true;

    /// <summary>Shows the private message pages.</summary>
    public bool AllowPrivateMessages { get; set; } = true;

    /// <summary>Shows "3 hours ago" instead of a clock time.</summary>
    public bool RelativeDates { get; set; } = true;

    /// <summary>Publishes the RSS feeds.</summary>
    public bool FeedsEnabled { get; set; } = true;

    /// <summary>Shows the signature stored on the member under each of their posts.</summary>
    public bool SignaturesEnabled { get; set; } = true;

    /// <summary>Topics listed on a forum page.</summary>
    public int TopicsPageSize { get; set; } = 20;

    /// <summary>Posts listed on a topic page.</summary>
    public int PostsPageSize { get; set; } = 15;

    /// <summary>Search results per page.</summary>
    public int SearchPageSize { get; set; } = 20;

    /// <summary>Active topics per page.</summary>
    public int ActivePageSize { get; set; } = 20;

    /// <summary>Private messages per page.</summary>
    public int MessagesPageSize { get; set; } = 20;

    /// <summary>Subscriptions per page.</summary>
    public int SubscriptionsPageSize { get; set; } = 20;

    /// <summary>Longest topic subject the form will accept.</summary>
    public int SubjectMaxLength { get; set; } = 255;

    /// <summary>Longest post the form will accept.</summary>
    public int PostMaxLength { get; set; } = 8000;

    /// <summary>Longest private-message subject.</summary>
    public int MessageSubjectMaxLength { get; set; } = 128;

    /// <summary>Longest private-message body.</summary>
    public int MessageTextMaxLength { get; set; } = 4000;

    /// <summary>Search terms shorter than this are rejected.</summary>
    public int SearchMinimumLength { get; set; } = 2;

    /// <summary>Items in an RSS feed.</summary>
    public int FeedCount { get; set; } = 20;

    /// <summary>Markdown, rich text, or both. Rich text needs a Pickstack editor registered by the host.</summary>
    public ForumEditorMode EditorMode { get; set; } = ForumEditorMode.Both;

    /// <summary>
    /// When false, forum posts do not open the host's file manager. The rich text editor keeps its own
    /// insert dialog, and the Markdown image button uploads a file from the author's computer.
    /// Other editors on the same site are unchanged.
    /// </summary>
    public bool UseFileManager { get; set; } = true;

    /// <summary>The editor a new post opens in when both are available.</summary>
    public ContentFormat DefaultContentFormat { get; set; } = ContentFormat.Markdown;

    /// <summary>
    /// When false, posts show initials and the profile page has no photo control.
    /// When true, the registered <c>IProfileProvider</c> supplies the photo. The built-in provider stores it under <see cref="ProfilePhotoPath"/>.
    /// </summary>
    public bool ProfilePhotosEnabled { get; set; } = true;

    /// <summary>
    /// Folder under wwwroot used by the built-in profile provider.
    /// Nested folders such as <c>media/avatars</c> are allowed. A path that leaves wwwroot is rejected.
    /// </summary>
    public string ProfilePhotoPath { get; set; } = "forumz/avatars";

    /// <summary>
    /// Folder under wwwroot for images inserted into Markdown posts when the file manager is off.
    /// Nested folders such as <c>media/posts</c> are allowed. A path that leaves wwwroot is rejected.
    /// </summary>
    public string PostImagePath { get; set; } = "forumz/images";
}