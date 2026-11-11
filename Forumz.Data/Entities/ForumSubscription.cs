namespace Forumz.Data.Entities;

/// <summary>
/// A subscription. <see cref="TopicId"/> is 0 when the member follows the whole forum.
/// A topic subscription stores both the forum and the topic so the list can name them.
/// </summary>
public class ForumSubscription : BaseEntity<int>
{
    /// <summary>Empty for a single-tenant host.</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Subscriber.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Forum being followed. Also set on a topic subscription.</summary>
    public int ForumId { get; set; }

    /// <summary>Topic being followed. Zero when the subscription is for the whole forum.</summary>
    public int TopicId { get; set; }

    /// <summary>When the member subscribed, in UTC.</summary>
    public DateTime CreatedOnUtc { get; set; }
}

/// <summary>Maps <see cref="ForumSubscription"/>.</summary>
public class ForumSubscriptionMap : IEntityTypeConfiguration<ForumSubscription>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ForumSubscription> builder)
    {
        builder.ToTable(Constants.Tables.Subscriptions);
        builder.HasKey(subscription => subscription.Id);
        builder.Property(subscription => subscription.TenantId).IsRequired().HasMaxLength(Constants.MaxLengths.TenantId).IsUnicode(false);
        builder.Property(subscription => subscription.UserId).IsRequired().HasMaxLength(Constants.MaxLengths.UserId).IsUnicode(true);
        builder.HasIndex(subscription => new { subscription.TenantId, subscription.UserId, subscription.ForumId, subscription.TopicId }).IsUnique();
    }
}