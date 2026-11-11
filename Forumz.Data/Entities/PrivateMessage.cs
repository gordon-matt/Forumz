namespace Forumz.Data.Entities;

/// <summary>
/// A private message. Deleting hides it from that side; the row goes away only when both sides have deleted it.
/// </summary>
public class PrivateMessage : BaseEntity<int>
{
    /// <summary>Empty for a single-tenant host.</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Sender.</summary>
    public string FromUserId { get; set; } = string.Empty;

    /// <summary>Recipient.</summary>
    public string ToUserId { get; set; } = string.Empty;

    /// <summary>Subject line.</summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>Plain text body.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Whether the recipient has opened it.</summary>
    public bool IsRead { get; set; }

    /// <summary>Hidden from the sender's sent list.</summary>
    public bool IsDeletedByAuthor { get; set; }

    /// <summary>Hidden from the recipient's inbox.</summary>
    public bool IsDeletedByRecipient { get; set; }

    /// <summary>When it was sent, in UTC.</summary>
    public DateTime CreatedOnUtc { get; set; }
}

/// <summary>Maps <see cref="PrivateMessage"/>.</summary>
public class PrivateMessageMap : IEntityTypeConfiguration<PrivateMessage>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PrivateMessage> builder)
    {
        builder.ToTable(Constants.Tables.Messages);
        builder.HasKey(message => message.Id);
        builder.Property(message => message.TenantId).IsRequired().HasMaxLength(Constants.MaxLengths.TenantId).IsUnicode(false);
        builder.Property(message => message.FromUserId).IsRequired().HasMaxLength(Constants.MaxLengths.UserId).IsUnicode(true);
        builder.Property(message => message.ToUserId).IsRequired().HasMaxLength(Constants.MaxLengths.UserId).IsUnicode(true);
        builder.Property(message => message.Subject).IsRequired().HasMaxLength(Constants.MaxLengths.MessageSubject).IsUnicode(true);
        builder.Property(message => message.Text).IsRequired().IsUnicode(true);
        builder.HasIndex(message => new { message.TenantId, message.ToUserId, message.IsDeletedByRecipient, message.CreatedOnUtc });
        builder.HasIndex(message => new { message.TenantId, message.FromUserId, message.IsDeletedByAuthor });
    }
}
