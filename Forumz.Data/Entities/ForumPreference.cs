namespace Forumz.Data.Entities;

/// <summary>
/// One settings row per tenant. Created from <see cref="ForumzOptions"/> the first time that tenant is used.
/// An empty <see cref="TenantId"/> is the single-tenant host.
/// </summary>
public class ForumPreference : BaseEntity<int>
{
    /// <summary>Empty for a single-tenant host.</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Members may edit their own posts.</summary>
    public bool AllowUsersToEditPosts { get; set; } = true;

    /// <summary>Members may delete their own posts.</summary>
    public bool AllowUsersToDeletePosts { get; set; } = true;

    /// <summary>Members may subscribe.</summary>
    public bool AllowUsersToManageSubscriptions { get; set; } = true;

    /// <summary>Private messages are available.</summary>
    public bool AllowPrivateMessages { get; set; } = true;

    /// <summary>Show relative times.</summary>
    public bool RelativeDates { get; set; } = true;

    /// <summary>Publish RSS feeds.</summary>
    public bool FeedsEnabled { get; set; } = true;

    /// <summary>Show signatures under posts.</summary>
    public bool SignaturesEnabled { get; set; } = true;

    /// <summary>Topics per forum page.</summary>
    public int TopicsPageSize { get; set; } = 20;

    /// <summary>Posts per topic page.</summary>
    public int PostsPageSize { get; set; } = 15;

    /// <summary>Search results per page.</summary>
    public int SearchPageSize { get; set; } = 20;

    /// <summary>Active topics per page.</summary>
    public int ActivePageSize { get; set; } = 20;

    /// <summary>Messages per page.</summary>
    public int MessagesPageSize { get; set; } = 20;

    /// <summary>Subscriptions per page.</summary>
    public int SubscriptionsPageSize { get; set; } = 20;

    /// <summary>Longest topic subject.</summary>
    public int SubjectMaxLength { get; set; } = 255;

    /// <summary>Longest post.</summary>
    public int PostMaxLength { get; set; } = 8000;

    /// <summary>Longest private-message subject.</summary>
    public int MessageSubjectMaxLength { get; set; } = 128;

    /// <summary>Longest private-message body.</summary>
    public int MessageTextMaxLength { get; set; } = 4000;

    /// <summary>Shortest search that is accepted.</summary>
    public int SearchMinimumLength { get; set; } = 2;

    /// <summary>Items in a feed.</summary>
    public int FeedCount { get; set; } = 20;
}

/// <summary>Maps <see cref="ForumPreference"/>.</summary>
public class ForumPreferenceMap : IEntityTypeConfiguration<ForumPreference>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ForumPreference> builder)
    {
        builder.ToTable(Constants.Tables.Preferences);
        builder.HasKey(preference => preference.Id);
        builder.Property(preference => preference.TenantId).IsRequired().HasMaxLength(Constants.MaxLengths.TenantId).IsUnicode(false);
        builder.HasIndex(preference => preference.TenantId).IsUnique();
    }
}