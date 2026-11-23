using Forumz.Models;
using Forumz.Services;

namespace Forumz.Sample;

/// <summary>Creates a welcome forum the first time the sample database is empty, and a profile for each demo user.</summary>
public sealed class SampleSeeder(IServiceProvider services, ILogger<SampleSeeder> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var forums = scope.ServiceProvider.GetRequiredService<IForumService>();
        var accessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();

        var index = await forums.GetIndexAsync(cancellationToken);
        if (!index.IsSuccess)
        {
            logger.LogWarning("Sample forum was not seeded: {Message}", index.Errors.FirstOrDefault());
            return;
        }

        if (index.Value.Groups.Count == 0)
        {
            await SeedWelcomeAsync(forums, accessor, scope.ServiceProvider, cancellationToken);
        }

        // A private message can only be addressed to someone who already has a profile.
        // Touching each account keeps the sign-in name as the name the message form accepts.
        foreach (var user in DemoUsers.All)
        {
            DemoUsers.Impersonate(accessor, scope.ServiceProvider, user);
            var profile = await forums.GetProfileAsync(cancellationToken);
            if (!profile.IsSuccess)
            {
                logger.LogWarning("Sample member {Name} was not created: {Message}", user.DisplayName, profile.Errors.FirstOrDefault());
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SeedWelcomeAsync(
        IForumService forums,
        IHttpContextAccessor accessor,
        IServiceProvider scope,
        CancellationToken cancellationToken)
    {
        DemoUsers.Impersonate(accessor, scope, DemoUsers.Administrator);
        var group = await forums.SaveGroupAsync(new SaveForumGroupRequest(null, "General", 1), cancellationToken);
        if (!group.IsSuccess)
        {
            logger.LogWarning("Sample forum was not seeded: {Message}", group.Errors.FirstOrDefault());
            return;
        }

        var forum = await forums.SaveForumAsync(
            new SaveForumRequest(null, group.Value, "Welcome", "A place to try Forumz.", 1),
            cancellationToken);
        if (!forum.IsSuccess)
        {
            return;
        }

        var topic = await forums.CreateTopicAsync(
            new CreateTopicRequest(
                forum.Value,
                "Hello",
                "This sample signs in as Dev User. Use the top bar to switch to Blair Chen or Casey Nguyen, then open Messages to write to either of them. Posts are plain text."),
            null,
            cancellationToken);
        if (!topic.IsSuccess)
        {
            return;
        }

        var blair = DemoUsers.Find("blair");
        var casey = DemoUsers.Find("casey");
        if (blair is not null)
        {
            await ReplyAsync(forums, accessor, scope, blair, topic.Value, "Open Messages and write to me there.", cancellationToken);
        }

        if (casey is not null)
        {
            await ReplyAsync(forums, accessor, scope, casey, topic.Value, "Or send one to me.", cancellationToken);
        }
    }

    private async Task ReplyAsync(
        IForumService forums,
        IHttpContextAccessor accessor,
        IServiceProvider scope,
        DemoUser user,
        int topicId,
        string text,
        CancellationToken cancellationToken)
    {
        DemoUsers.Impersonate(accessor, scope, user);
        var reply = await forums.CreatePostAsync(new CreatePostRequest(topicId, text), null, cancellationToken);
        if (!reply.IsSuccess)
        {
            logger.LogWarning("Sample reply by {Name} was not posted: {Message}", user.DisplayName, reply.Errors.FirstOrDefault());
        }
    }
}