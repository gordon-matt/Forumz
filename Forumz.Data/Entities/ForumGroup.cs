namespace Forumz.Data.Entities;

/// <summary>A heading that groups forums. The tenant id is empty when the host is not multi-tenant.</summary>
public class ForumGroup : BaseEntity<int>
{
    /// <summary>Empty for a single-tenant host. A multi-tenant host stamps its current tenant.</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Group name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Lower numbers are listed first.</summary>
    public int DisplayOrder { get; set; }

    /// <summary>When the group was created, in UTC.</summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>When the group was last renamed or reordered, in UTC.</summary>
    public DateTime UpdatedOnUtc { get; set; }
}

/// <summary>Maps <see cref="ForumGroup"/>.</summary>
public class ForumGroupMap : IEntityTypeConfiguration<ForumGroup>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ForumGroup> builder)
    {
        builder.ToTable(Constants.Tables.Groups);
        builder.HasKey(group => group.Id);
        builder.Property(group => group.TenantId).IsRequired().HasMaxLength(Constants.MaxLengths.TenantId).IsUnicode(false);
        builder.Property(group => group.Name).IsRequired().HasMaxLength(Constants.MaxLengths.Name).IsUnicode(true);
        builder.Property(group => group.DisplayOrder).IsRequired();
        builder.Property(group => group.CreatedOnUtc).IsRequired();
        builder.Property(group => group.UpdatedOnUtc).IsRequired();
        builder.HasIndex(group => new { group.TenantId, group.DisplayOrder });
    }
}
