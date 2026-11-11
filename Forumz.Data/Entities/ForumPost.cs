namespace Forumz.Data.Entities;

/// <summary>A post. The body is plain text; the page encodes it when rendering.</summary>
public class ForumPost : BaseEntity<int>
{
    /// <summary>Topic the post belongs to.</summary>
    public int TopicId { get; set; }

    /// <summary>Host user id of the author.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Source body: Markdown or HTML, according to <see cref="Format"/>.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Whether <see cref="Text"/> is Markdown or HTML from the rich text editor.</summary>
    public ContentFormat Format { get; set; } = ContentFormat.Markdown;

    /// <summary>Remote address captured when the circuit opened. Empty when the host had none.</summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>When the post was written, in UTC.</summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>When the post was last edited, in UTC.</summary>
    public DateTime UpdatedOnUtc { get; set; }
}

/// <summary>Maps <see cref="ForumPost"/>.</summary>
public class ForumPostMap : IEntityTypeConfiguration<ForumPost>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ForumPost> builder)
    {
        builder.ToTable(Constants.Tables.Posts);
        builder.HasKey(post => post.Id);
        builder.Property(post => post.TopicId).IsRequired();
        builder.Property(post => post.UserId).IsRequired().HasMaxLength(Constants.MaxLengths.UserId).IsUnicode(true);
        builder.Property(post => post.Text).IsRequired().IsUnicode(true);
        builder.Property(post => post.Format).HasConversion<int>().HasDefaultValue(ContentFormat.Markdown);
        builder.Property(post => post.IpAddress).IsRequired().HasMaxLength(Constants.MaxLengths.IpAddress).IsUnicode(false);
        builder.HasIndex(post => new { post.TopicId, post.CreatedOnUtc });
        builder.HasOne<ForumTopic>().WithMany().HasForeignKey(post => post.TopicId).OnDelete(DeleteBehavior.Restrict);
    }
}