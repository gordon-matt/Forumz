namespace Forumz.Data.Entities;

/// <summary>
/// The forum's view of a host user. Forumz does not own the account; it remembers the name to
/// show and how many posts they have written.
/// </summary>
public class ForumMember : BaseEntity<int>
{
    /// <summary>Empty for a single-tenant host.</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Host user id.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Name shown on posts.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Plain text shown under the member's posts when signatures are enabled.</summary>
    public string Signature { get; set; } = string.Empty;

    /// <summary>Posts the member has written in this tenant.</summary>
    public int PostCount { get; set; }

    /// <summary>When the profile was first stored, in UTC.</summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>When the name, signature, or post count last changed, in UTC.</summary>
    public DateTime UpdatedOnUtc { get; set; }
}

/// <summary>Maps <see cref="ForumMember"/>.</summary>
public class ForumMemberMap : IEntityTypeConfiguration<ForumMember>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ForumMember> builder)
    {
        builder.ToTable(Constants.Tables.Members);
        builder.HasKey(member => member.Id);
        builder.Property(member => member.TenantId).IsRequired().HasMaxLength(Constants.MaxLengths.TenantId).IsUnicode(false);
        builder.Property(member => member.UserId).IsRequired().HasMaxLength(Constants.MaxLengths.UserId).IsUnicode(true);
        builder.Property(member => member.DisplayName).IsRequired().HasMaxLength(Constants.MaxLengths.DisplayName).IsUnicode(true);
        builder.Property(member => member.Signature).IsRequired().HasMaxLength(Constants.MaxLengths.Signature).IsUnicode(true);
        builder.HasIndex(member => new { member.TenantId, member.UserId }).IsUnique();
    }
}