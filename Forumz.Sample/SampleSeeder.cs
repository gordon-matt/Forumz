using Forumz.Models;
using Forumz.Services;

namespace Forumz.Sample;

/// <summary>Creates a welcome forum the first time the sample database is empty.</summary>
public sealed class SampleSeeder(IServiceProvider services, ILogger<SampleSeeder> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var forums = scope.ServiceProvider.GetRequiredService<IForumService>();
        var index = await forums.GetIndexAsync(cancellationToken);
        if (!index.IsSuccess || index.Value.Groups.Count > 0)
        {
            return;
        }

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

        await forums.CreateTopicAsync(
            new CreateTopicRequest(forum.Value, "Hello", "This sample is signed in as Dev User. Posts are plain text."),
            null,
            cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}