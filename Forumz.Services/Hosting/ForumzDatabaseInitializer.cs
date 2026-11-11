using Microsoft.Extensions.Hosting;

namespace Forumz.Services.Hosting;

/// <summary>Applies Forumz migrations before the host serves requests. A missing provider does not stop the host.</summary>
public sealed class ForumzDatabaseInitializer(
    ILogger<ForumzDatabaseInitializer> logger,
    IServiceProvider services,
    IOptions<ForumzOptions> options) : IHostedService
{
    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = services.GetService<IForumDbContextFactory>();
        if (factory is null)
        {
            logger.LogWarning(
                "Forumz has no storage provider, so forums will not be stored. Call AddForumzSqlite, AddForumzSqlServer, AddForumzNpgsql or AddForumzMySql.");
            return;
        }

        if (!options.Value.AutoMigrate)
        {
            return;
        }

        try
        {
            await using var context = factory.GetContext();
            await context.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Forumz prepared its {Provider} database", factory.ProviderName);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Forumz could not prepare its {Provider} database", factory.ProviderName);
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}