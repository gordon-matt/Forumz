namespace Forumz.Core;

/// <summary>How a post is written. Stored as an integer, so the values do not change.</summary>
public enum ContentFormat
{
    /// <summary>Markdown, including the plain text a reader types with no formatting.</summary>
    Markdown = 0,

    /// <summary>HTML from the host's rich text editor.</summary>
    Html = 1,
}

/// <summary>Which editors a host offers. Without a rich text editor, rich text falls back to Markdown.</summary>
public enum ForumEditorMode
{
    /// <summary>The author chooses for each post.</summary>
    Both = 0,

    /// <summary>Markdown only.</summary>
    Markdown = 1,

    /// <summary>The host's rich text editor. Markdown is used when none is registered.</summary>
    RichText = 2,
}