namespace Forumz.Services;

public sealed partial class ForumService
{
    /// <inheritdoc />
    public Task<Result> SubscribeToForumAsync(int forumId, CancellationToken cancellationToken = default) =>
        SubscribeAsync(forumId, 0, cancellationToken);

    /// <inheritdoc />
    public async Task<Result> SubscribeToTopicAsync(int topicId, CancellationToken cancellationToken = default)
    {
        var found = await FindTopicAsync(topicId, cancellationToken);
        return !found.IsSuccess ? found.ToVoid() : await SubscribeAsync(found.Value.Forum.Id, topicId, cancellationToken);
    }

    private async Task<Result> SubscribeAsync(int forumId, int topicId, CancellationToken cancellationToken)
    {
        var writer = RequireWriter();
        if (!writer.IsSuccess)
        {
            return writer.ToVoid();
        }

        var preference = await PreferenceAsync(cancellationToken);
        if (!CanSubscribe(preference))
        {
            return Result.Forbidden("Subscriptions are turned off.");
        }

        if (topicId == 0)
        {
            var forum = await FindForumAsync(forumId, cancellationToken);
            if (!forum.IsSuccess)
            {
                return forum.ToVoid();
            }
        }

        string userId = writer.Value;
        var existing = await FindSubscriptionAsync(userId, forumId, topicId, cancellationToken);
        if (existing is not null)
        {
            return Result.Success();
        }

        await subscriptions.InsertAsync(new ForumSubscription
        {
            TenantId = Tenant,
            UserId = userId,
            ForumId = forumId,
            TopicId = topicId,
            CreatedOnUtc = Now,
        }, Token(cancellationToken));

        logger.LogInformation("User {UserId} subscribed to forum {ForumId} topic {TopicId}", userId, forumId, topicId);
        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> UnsubscribeAsync(int subscriptionId, CancellationToken cancellationToken = default)
    {
        var writer = RequireWriter();
        if (!writer.IsSuccess)
        {
            return writer.ToVoid();
        }

        var subscription = await subscriptions.FindOneAsync(new SearchOptions<ForumSubscription>
        {
            Query = item => item.Id == subscriptionId && item.TenantId == Tenant,
            CancellationToken = cancellationToken,
        });

        if (subscription is null || !string.Equals(subscription.UserId, writer.Value, StringComparison.Ordinal))
        {
            return Result.NotFound("That subscription was not found.");
        }

        await subscriptions.DeleteAsync(subscription, Token(cancellationToken));
        logger.LogInformation("User {UserId} removed subscription {SubscriptionId}", writer.Value, subscriptionId);
        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result<SubscriptionListModel>> GetSubscriptionsAsync(int pageNumber, CancellationToken cancellationToken = default)
    {
        var writer = RequireWriter();
        if (!writer.IsSuccess)
        {
            return writer.To<SubscriptionListModel>();
        }

        var preference = await PreferenceAsync(cancellationToken);
        int page = PageOf(pageNumber);
        var pageItems = await subscriptions.FindAsync(new SearchOptions<ForumSubscription>
        {
            Query = item => item.TenantId == Tenant && item.UserId == writer.Value,
            OrderBy = query => query.OrderByDescending(item => item.CreatedOnUtc),
            PageNumber = page,
            PageSize = preference.SubscriptionsPageSize,
            CancellationToken = cancellationToken,
        });

        var forumIds = pageItems.Select(item => item.ForumId).Distinct().ToList();
        var topicIds = pageItems.Where(item => item.TopicId != 0).Select(item => item.TopicId).Distinct().ToList();
        var forumPage = forumIds.Count == 0
            ? null
            : await forums.FindAsync(new SearchOptions<Forum>
            {
                Query = forum => forumIds.Contains(forum.Id),
                CancellationToken = cancellationToken,
            });
        var topicPage = topicIds.Count == 0
            ? null
            : await topics.FindAsync(new SearchOptions<ForumTopic>
            {
                Query = topic => topicIds.Contains(topic.Id),
                CancellationToken = cancellationToken,
            });

        var forumNames = (forumPage ?? Enumerable.Empty<Forum>()).ToDictionary(forum => forum.Id, forum => forum.Name);
        var subjects = (topicPage ?? Enumerable.Empty<ForumTopic>()).ToDictionary(topic => topic.Id, topic => topic.Subject);

        var items = pageItems.Select(item => new SubscriptionModel(
            item.Id,
            item.ForumId,
            forumNames.GetValueOrDefault(item.ForumId, "Forum"),
            item.TopicId == 0 ? null : item.TopicId,
            item.TopicId == 0 ? null : subjects.GetValueOrDefault(item.TopicId),
            item.CreatedOnUtc)).ToList();

        return Result<SubscriptionListModel>.Success(new SubscriptionListModel(items, page, preference.SubscriptionsPageSize, pageItems.ItemCount));
    }
}
