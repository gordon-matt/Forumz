namespace Forumz.Data.Entities;

/// <summary>A topic. <see cref="NumPosts"/> includes the opening post, so the reply count is one less.</summary>
public class ForumTopic : BaseEntity<int>
{
    /// <summary>Forum that holds the topic.</summary>
    public int ForumId { get; set; }

    /// <summary>Host user id of whoever started the topic.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Normal, sticky, or announcement.</summary>
    public ForumTopicType TopicType { get; set; } = ForumTopicType.Normal;

    /// <summary>Subject line.</summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>Posts in the topic, including the opening post.</summary>
    public int NumPosts { get; set; }

    /// <summary>Times the topic has been opened.</summary>
    public int Views { get; set; }

    /// <summary>Latest post. Zero until the opening post is stored.</summary>
    public int LastPostId { get; set; }

    /// <summary>Author of the latest post.</summary>
    public string LastPostUserId { get; set; } = string.Empty;

    /// <summary>When the latest post was written, in UTC.</summary>
    public DateTime? LastPostTime { get; set; }

    /// <summary>When the topic was started, in UTC.</summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>When the topic was last changed, in UTC.</summary>
    public DateTime UpdatedOnUtc { get; set; }

    /// <summary>Posts after the opening post.</summary>
    public int NumReplies => NumPosts > 0 ? NumPosts - 1 : 0;
}

/// <summary>Maps a topic. The type is stored as its byte value, and deleting the forum is restricted.</summary>
public class ForumTopicMap : IEntityTypeConfiguration<ForumTopic>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ForumTopic> builder)
    {
        builder.ToTable(Constants.Tables.Topics);
        builder.HasKey(topic => topic.Id);
        builder.Property(topic => topic.ForumId).IsRequired();
        builder.Property(topic => topic.UserId).IsRequired().HasMaxLength(Constants.MaxLengths.UserId).IsUnicode(true);
        builder.Property(topic => topic.TopicType).IsRequired().HasConversion<byte>();
        builder.Property(topic => topic.Subject).IsRequired().HasMaxLength(Constants.MaxLengths.Subject).IsUnicode(true);
        builder.Property(topic => topic.LastPostUserId).IsRequired().HasMaxLength(Constants.MaxLengths.UserId).IsUnicode(true);
        builder.Ignore(topic => topic.NumReplies);
        builder.HasIndex(topic => new { topic.ForumId, topic.TopicType, topic.LastPostTime });
        builder.HasOne<Forum>().WithMany().HasForeignKey(topic => topic.ForumId).OnDelete(DeleteBehavior.Restrict);
    }
}