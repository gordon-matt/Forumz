namespace Forumz.Data.Entities;

/// <summary>A forum inside a group. Counts are maintained by the service so the index does not scan posts.</summary>
public class Forum : BaseEntity<int>
{
    /// <summary>Owning group.</summary>
    public int ForumGroupId { get; set; }

    /// <summary>Forum name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Shown under the name. Empty when none was set.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Topics currently in the forum.</summary>
    public int NumTopics { get; set; }

    /// <summary>Posts currently in the forum, including each topic's first post.</summary>
    public int NumPosts { get; set; }

    /// <summary>Topic of the latest post. Zero when the forum is empty.</summary>
    public int LastTopicId { get; set; }

    /// <summary>Latest post. Zero when the forum is empty.</summary>
    public int LastPostId { get; set; }

    /// <summary>Author of the latest post. Empty when the forum is empty.</summary>
    public string LastPostUserId { get; set; } = string.Empty;

    /// <summary>When the latest post was written, in UTC.</summary>
    public DateTime? LastPostTime { get; set; }

    /// <summary>Lower numbers are listed first inside the group.</summary>
    public int DisplayOrder { get; set; }

    /// <summary>When the forum was created, in UTC.</summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>When the forum was last changed, in UTC.</summary>
    public DateTime UpdatedOnUtc { get; set; }
}

/// <summary>Maps <see cref="Forum"/>.</summary>
public class ForumMap : IEntityTypeConfiguration<Forum>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Forum> builder)
    {
        builder.ToTable(Constants.Tables.Forums);
        builder.HasKey(forum => forum.Id);
        builder.Property(forum => forum.ForumGroupId).IsRequired();
        builder.Property(forum => forum.Name).IsRequired().HasMaxLength(Constants.MaxLengths.Name).IsUnicode(true);
        builder.Property(forum => forum.Description).IsRequired().HasMaxLength(Constants.MaxLengths.Description).IsUnicode(true);
        builder.Property(forum => forum.LastPostUserId).IsRequired().HasMaxLength(Constants.MaxLengths.UserId).IsUnicode(true);
        builder.HasIndex(forum => new { forum.ForumGroupId, forum.DisplayOrder });
        builder.HasOne<ForumGroup>().WithMany().HasForeignKey(forum => forum.ForumGroupId).OnDelete(DeleteBehavior.Restrict);
    }
}