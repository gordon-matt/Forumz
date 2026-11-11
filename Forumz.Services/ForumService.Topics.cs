namespace Forumz.Services;

public sealed partial class ForumService
{
    /// <inheritdoc />
    public async Task<Result<ForumIndexModel>> GetIndexAsync(CancellationToken cancellationToken = default)
    {
        var reader = RequireReader();
        if (!reader.IsSuccess)
        {
            return reader.To<ForumIndexModel>();
        }

        var groupPage = await groups.FindAsync(new SearchOptions<ForumGroup>
        {
            Query = group => group.TenantId == Tenant,
            OrderBy = query => query.OrderBy(group => group.DisplayOrder).ThenBy(group => group.Name),
            CancellationToken = cancellationToken,
        });

        var groupIds = groupPage.Select(group => group.Id).ToList();
        var forumPage = groupIds.Count == 0
            ? null
            : await forums.FindAsync(new SearchOptions<Forum>
            {
                Query = forum => groupIds.Contains(forum.ForumGroupId),
                OrderBy = query => query.OrderBy(forum => forum.DisplayOrder).ThenBy(forum => forum.Name),
                CancellationToken = cancellationToken,
            });

        var names = await MembersAsync(forumPage?.Select(forum => forum.LastPostUserId) ?? [], cancellationToken);
        var forumsByGroup = (forumPage ?? Enumerable.Empty<Forum>()).GroupBy(forum => forum.ForumGroupId).ToDictionary(group => group.Key, group => group.ToList());

        var models = groupPage.Select(group => new ForumGroupModel(
            group.Id,
            group.Name,
            forumsByGroup.TryGetValue(group.Id, out var rows)
                ? rows.Select(forum => new ForumRowModel(
                    forum.Id,
                    forum.Name,
                    forum.Description,
                    forum.NumTopics,
                    forum.NumPosts,
                    forum.LastTopicId == 0 ? null : forum.LastTopicId,
                    string.IsNullOrEmpty(forum.LastPostUserId) ? null : NameOf(names, forum.LastPostUserId),
                    forum.LastPostTime)).ToList()
                : [])).ToList();

        return Result<ForumIndexModel>.Success(new ForumIndexModel(models, access.CanManage()));
    }

    /// <inheritdoc />
    public async Task<Result<ForumPageModel>> GetForumAsync(int forumId, int pageNumber, CancellationToken cancellationToken = default)
    {
        var reader = RequireReader();
        if (!reader.IsSuccess)
        {
            return reader.To<ForumPageModel>();
        }

        var found = await FindForumAsync(forumId, cancellationToken);
        if (!found.IsSuccess)
        {
            return found.To<ForumPageModel>();
        }

        var preference = await PreferenceAsync(cancellationToken);
        int page = PageOf(pageNumber);
        var topicPage = await topics.FindAsync(new SearchOptions<ForumTopic>
        {
            Query = topic => topic.ForumId == forumId,
            OrderBy = query => query
                .OrderByDescending(topic => topic.TopicType)
                .ThenByDescending(topic => topic.LastPostTime)
                .ThenByDescending(topic => topic.Id),
            PageNumber = page,
            PageSize = preference.TopicsPageSize,
            CancellationToken = cancellationToken,
        });

        var names = await MembersAsync(topicPage.SelectMany<ForumTopic, string?>(topic => [topic.UserId, topic.LastPostUserId]), cancellationToken);
        string? userId = users.GetCurrentUserId();
        var subscription = userId is null ? null : await FindSubscriptionAsync(userId, forumId, 0, cancellationToken);
        var (forum, group) = found.Value;

        return Result<ForumPageModel>.Success(new ForumPageModel(
            forum.Id,
            group.Id,
            group.Name,
            forum.Name,
            forum.Description,
            access.CanWrite(),
            CanSubscribe(preference),
            subscription is not null,
            subscription?.Id,
            topicPage.Select(topic => ToRow(topic, forum.Name, names)).ToList(),
            page,
            preference.TopicsPageSize,
            topicPage.ItemCount));
    }

    /// <inheritdoc />
    public async Task<Result<TopicPageModel>> GetTopicAsync(int topicId, int pageNumber, bool countView, CancellationToken cancellationToken = default)
    {
        var reader = RequireReader();
        if (!reader.IsSuccess)
        {
            return reader.To<TopicPageModel>();
        }

        var found = await FindTopicAsync(topicId, cancellationToken);
        if (!found.IsSuccess)
        {
            return found.To<TopicPageModel>();
        }

        var (topic, forum, group) = found.Value;
        if (countView)
        {
            topic.Views += 1;
            topic = await topics.UpdateAsync(topic, Token(cancellationToken));
        }

        var preference = await PreferenceAsync(cancellationToken);
        int page = PageOf(pageNumber);
        var postPage = await posts.FindAsync(new SearchOptions<ForumPost>
        {
            Query = post => post.TopicId == topicId,
            OrderBy = query => query.OrderBy(post => post.CreatedOnUtc).ThenBy(post => post.Id),
            PageNumber = page,
            PageSize = preference.PostsPageSize,
            CancellationToken = cancellationToken,
        });

        var names = await MembersAsync(postPage.Select(post => post.UserId), cancellationToken);
        string? userId = users.GetCurrentUserId();
        var subscription = userId is null ? null : await FindSubscriptionAsync(userId, forum.Id, topic.Id, cancellationToken);

        var postModels = postPage.Select(post =>
        {
            names.TryGetValue(post.UserId, out var member);
            return new PostModel(
                post.Id,
                post.UserId,
                NameOf(names, post.UserId),
                post.Text,
                preference.SignaturesEnabled && member is { Signature.Length: > 0 } ? member.Signature : null,
                post.CreatedOnUtc,
                post.UpdatedOnUtc,
                CanEdit(post, preference),
                CanDelete(post, preference));
        }).ToList();

        return Result<TopicPageModel>.Success(new TopicPageModel(
            topic.Id,
            forum.Id,
            forum.Name,
            group.Id,
            group.Name,
            topic.Subject,
            topic.TopicType,
            topic.Views,
            access.CanWrite(),
            access.CanModerate(),
            CanSubscribe(preference),
            subscription is not null,
            subscription?.Id,
            postModels,
            page,
            preference.PostsPageSize,
            postPage.ItemCount));
    }

    /// <inheritdoc />
    public Task<Result<TopicListModel>> GetActiveAsync(int? forumId, int pageNumber, CancellationToken cancellationToken = default) =>
        ListTopicsAsync(forumId, null, pageNumber, "Active discussions", null, cancellationToken);

    /// <inheritdoc />
    public async Task<Result<TopicListModel>> SearchAsync(string keywords, int? forumId, int pageNumber, CancellationToken cancellationToken = default)
    {
        var reader = RequireReader();
        if (!reader.IsSuccess)
        {
            return reader.To<TopicListModel>();
        }

        var preference = await PreferenceAsync(cancellationToken);
        string term = keywords?.Trim() ?? string.Empty;
        if (term.Length < preference.SearchMinimumLength)
        {
            return Result<TopicListModel>.Invalid(new ValidationError(
                nameof(keywords),
                $"Enter at least {preference.SearchMinimumLength} characters."));
        }

        return await ListTopicsAsync(forumId, term.ToLowerInvariant(), pageNumber, $"Results for \"{term}\"", null, cancellationToken);
    }

    private async Task<Result<TopicListModel>> ListTopicsAsync(
        int? forumId,
        string? term,
        int pageNumber,
        string title,
        int? pageSizeOverride,
        CancellationToken cancellationToken)
    {
        var reader = RequireReader();
        if (!reader.IsSuccess)
        {
            return reader.To<TopicListModel>();
        }

        if (forumId is int selected)
        {
            var forum = await FindForumAsync(selected, cancellationToken);
            if (!forum.IsSuccess)
            {
                return forum.To<TopicListModel>();
            }
        }

        var preference = await PreferenceAsync(cancellationToken);
        int page = PageOf(pageNumber);
        int pageSize = pageSizeOverride ?? (term is null ? preference.ActivePageSize : preference.SearchPageSize);
        string tenant = Tenant;

        using var topicConnection = topics.OpenConnection(Token(cancellationToken));
        using var forumConnection = forums.UseConnection(topicConnection);
        using var groupConnection = groups.UseConnection(topicConnection);
        using var postConnection = term is null ? null : posts.UseConnection(topicConnection);

        var query = topicConnection.Query().Where(topic =>
            forumConnection.Query().Any(forum =>
                forum.Id == topic.ForumId
                && groupConnection.Query().Any(group => group.Id == forum.ForumGroupId && group.TenantId == tenant)));

        if (forumId is int onlyForum)
        {
            query = query.Where(topic => topic.ForumId == onlyForum);
        }

        if (term is not null && postConnection is not null)
        {
            query = query.Where(topic =>
                topic.Subject.ToLower().Contains(term)
                || postConnection.Query().Any(post => post.TopicId == topic.Id && post.Text.ToLower().Contains(term)));
        }

        int total = await query.CountAsync(cancellationToken);
        var pageItems = await query
            .OrderByDescending(topic => topic.TopicType)
            .ThenByDescending(topic => topic.LastPostTime)
            .ThenByDescending(topic => topic.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var forumIds = pageItems.Select(topic => topic.ForumId).Distinct().ToList();
        var forumPage = forumIds.Count == 0
            ? null
            : await forums.FindAsync(new SearchOptions<Forum>
            {
                Query = forum => forumIds.Contains(forum.Id),
                CancellationToken = cancellationToken,
            });
        var forumNames = (forumPage ?? Enumerable.Empty<Forum>()).ToDictionary(forum => forum.Id, forum => forum.Name);
        var names = await MembersAsync(pageItems.SelectMany<ForumTopic, string?>(topic => [topic.UserId, topic.LastPostUserId]), cancellationToken);

        return Result<TopicListModel>.Success(new TopicListModel(
            title,
            pageItems.Select(topic => ToRow(topic, forumNames.GetValueOrDefault(topic.ForumId, "Forum"), names)).ToList(),
            page,
            pageSize,
            total));
    }

    /// <inheritdoc />
    public async Task<Result<FeedModel>> GetFeedAsync(int? forumId, CancellationToken cancellationToken = default)
    {
        var reader = RequireReader();
        if (!reader.IsSuccess)
        {
            return reader.To<FeedModel>();
        }

        var preference = await PreferenceAsync(cancellationToken);
        if (!preference.FeedsEnabled)
        {
            return Result<FeedModel>.NotFound("Feeds are turned off.");
        }

        string title = options.Value.Title;
        string description = "Latest discussions";
        if (forumId is int selected)
        {
            var forum = await FindForumAsync(selected, cancellationToken);
            if (!forum.IsSuccess)
            {
                return forum.To<FeedModel>();
            }

            title = forum.Value.Forum.Name;
            description = forum.Value.Forum.Description;
        }

        var listed = await ListTopicsAsync(forumId, null, 1, title, preference.FeedCount, cancellationToken);
        if (!listed.IsSuccess)
        {
            return listed.To<FeedModel>();
        }

        var items = listed.Value.Topics
            .Take(preference.FeedCount)
            .Select(topic => new FeedItemModel(topic.Subject, $"topic/{topic.Id}", topic.Subject, topic.LastPostTime ?? DateTime.UtcNow))
            .ToList();

        return Result<FeedModel>.Success(new FeedModel(title, description, items));
    }
}
