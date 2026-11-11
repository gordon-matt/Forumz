namespace Forumz.Core;

/// <summary>
/// How a topic is ordered. The numeric values are the sort key, so a higher value stays above a lower one.
/// </summary>
public enum ForumTopicType : byte
{
    /// <summary>Ordered by the latest reply.</summary>
    Normal = 10,

    /// <summary>Kept above normal topics until a moderator clears it.</summary>
    Sticky = 15,

    /// <summary>Kept above sticky topics.</summary>
    Announcement = 20,
}