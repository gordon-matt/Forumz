namespace Forumz.Data;

/// <summary>
/// Provider-agnostic context. Each <c>Forumz.Data.{Provider}</c> project subclasses this and owns
/// its migrations, because EF stores provider-specific column types in them.
/// </summary>
public abstract class ForumzDbContextBase(DbContextOptions options) : DbContext(options)
{
    /// <summary>Gets the forum groups.</summary>
    public DbSet<ForumGroup> Groups => Set<ForumGroup>();

    /// <summary>Gets the forums.</summary>
    public DbSet<Forum> Forums => Set<Forum>();

    /// <summary>Gets the topics.</summary>
    public DbSet<ForumTopic> Topics => Set<ForumTopic>();

    /// <summary>Gets the posts.</summary>
    public DbSet<ForumPost> Posts => Set<ForumPost>();

    /// <summary>Gets the subscriptions.</summary>
    public DbSet<ForumSubscription> Subscriptions => Set<ForumSubscription>();

    /// <summary>Gets the private messages.</summary>
    public DbSet<PrivateMessage> Messages => Set<PrivateMessage>();

    /// <summary>Gets the member profiles.</summary>
    public DbSet<ForumMember> Members => Set<ForumMember>();

    /// <summary>Gets the settings rows.</summary>
    public DbSet<ForumPreference> Preferences => Set<ForumPreference>();

    /// <summary>
    /// Whether tables can live in <see cref="Constants.Schema"/>. MySQL and SQLite cannot, and
    /// receive <see cref="Constants.TablePrefix"/> on the table name instead.
    /// </summary>
    protected virtual bool SupportsSchemas => true;

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ForumzDbContextBase).Assembly);

        foreach (var property in builder.Model.GetEntityTypes().SelectMany(entity => entity.GetProperties()))
        {
            if (property.ClrType == typeof(DateTime))
            {
                property.SetValueConverter(UtcDateTimeConverter.Instance);
            }
            else if (property.ClrType == typeof(DateTime?))
            {
                property.SetValueConverter(UtcNullableDateTimeConverter.Instance);
            }
        }

        if (SupportsSchemas)
        {
            builder.HasDefaultSchema(Constants.Schema);
            return;
        }

        builder.Model.SetDefaultSchema(null);

        foreach (var entityType in builder.Model.GetEntityTypes().Where(entity => entity.BaseType is null))
        {
            if (entityType.GetTableName() is not { } tableName)
            {
                continue;
            }

            entityType.SetSchema(null);
            entityType.SetTableName($"{Constants.TablePrefix}{tableName}");
        }
    }
}
