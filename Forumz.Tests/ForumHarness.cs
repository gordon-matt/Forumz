using Extenso.Data.Entity;
using Forumz.Core;
using Forumz.Data;
using Forumz.Data.Entities;
using Forumz.Data.Sqlite;
using Forumz.Services;
using Heroicsoft.NetForge.Abstractions.Auth;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Forumz.Tests;

internal sealed class TestForumDatabase : IDisposable
{
    private readonly SqliteConnection keepAlive;

    public TestForumDatabase()
    {
        string connectionString = $"Data Source=forumz-tests-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        keepAlive = new SqliteConnection(connectionString);
        keepAlive.Open();
        Factory = new ForumzDbContextFactory(connectionString);
        using var context = Factory.GetContext();
        context.Database.Migrate();
    }

    public IForumDbContextFactory Factory { get; }

    public IRepository<TEntity> Repository<TEntity>()
        where TEntity : class, IEntity =>
        new EntityFrameworkRepository<TEntity>(Factory);

    public void Dispose() => keepAlive.Dispose();
}

internal sealed class TestUser : IUserContextService
{
    public string? UserId { get; set; } = "user-1";

    public string? Name { get; set; } = "Ada";

    public bool Administrator { get; set; }

    public string? GetCurrentUserId() => UserId;

    public string? GetCurrentUserName() => Name;

    public bool IsAuthenticated() => !string.IsNullOrEmpty(UserId);

    public bool IsAdministrator() => Administrator;

    public IReadOnlyList<string> GetRoleNames() => Administrator ? ["Administrator"] : [];
}

internal sealed class TestTenant : IForumTenantScope
{
    public string? TenantId { get; set; }
}

internal sealed class TestNotifier : IForumNotifier
{
    public List<ForumPostNotice> Notices { get; } = [];

    public Task NotifyAsync(ForumPostNotice notice, CancellationToken cancellationToken = default)
    {
        Notices.Add(notice);
        return Task.CompletedTask;
    }
}

internal static class ForumHarness
{
    public static ForumService Create(TestForumDatabase database, TestUser user, TestTenant? tenant = null, TestNotifier? notifier = null) =>
        new(
            NullLogger<ForumService>.Instance,
            Options.Create(new ForumzOptions()),
            user,
            new DefaultForumAccess(user),
            tenant ?? new TestTenant(),
            notifier ?? new TestNotifier(),
            database.Repository<ForumGroup>(),
            database.Repository<Forum>(),
            database.Repository<ForumTopic>(),
            database.Repository<ForumPost>(),
            database.Repository<ForumSubscription>(),
            database.Repository<PrivateMessage>(),
            database.Repository<ForumMember>(),
            database.Repository<ForumPreference>(),
            TimeProvider.System);
}
