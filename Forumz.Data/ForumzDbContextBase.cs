namespace Forumz.Data;

/// <summary>
/// Provider-agnostic context. Each <c>Forumz.Data.{Provider}</c> project subclasses this and owns
/// its migrations, because EF stores provider-specific column types in them.
/// </summary>
public abstract class ForumzDbContextBase(DbContextOptions options) : DbContext(options)
{
    /// <summary>Groups. A multi-tenant host filters these by tenant; an empty tenant id is the single site.</summary>
    public DbSet<ForumGroup> Groups => Set<ForumGroup>();

    /// <summary>Forums. Counts on each row are maintained by the service.</summary>
    public DbSet<Forum> Forums => Set<Forum>();

    /// <summary>Topics. <see cref="ForumTopic.NumPosts"/> includes the opening post.</summary>
    public DbSet<ForumTopic> Topics => Set<ForumTopic>();

    /// <summary>Posts. <see cref="ForumPost.Text"/> is Markdown or HTML, and the page renders the sanitized HTML.</summary>
    public DbSet<ForumPost> Posts => Set<ForumPost>();

    /// <summary>Forum and topic subscriptions. A topic id of 0 follows the whole forum.</summary>
    public DbSet<ForumSubscription> Subscriptions => Set<ForumSubscription>();

    /// <summary>Private messages. A row remains until both sides have deleted it.</summary>
    public DbSet<PrivateMessage> Messages => Set<PrivateMessage>();

    /// <summary>Display names, signatures, and post counts, keyed by the host's user id.</summary>
    public DbSet<ForumMember> Members => Set<ForumMember>();

    /// <summary>One settings row per tenant, created from the options the first time that tenant is used.</summary>
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