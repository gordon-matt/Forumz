namespace Forumz.Services;

/// <summary>Forum reads and writes for the current tenant.</summary>
public sealed partial class ForumService(
    ILogger<ForumService> logger,
    IOptions<ForumzOptions> options,
    IUserContextService users,
    IForumAccess access,
    IForumTenantScope tenants,
    IForumNotifier notifier,
    IRepository<ForumGroup> groups,
    IRepository<Forum> forums,
    IRepository<ForumTopic> topics,
    IRepository<ForumPost> posts,
    IRepository<ForumSubscription> subscriptions,
    IRepository<PrivateMessage> messages,
    IRepository<ForumMember> members,
    IRepository<ForumPreference> preferences,
    IForumContentRenderer content,
    TimeProvider time) : IForumService
{
    private string Tenant => string.IsNullOrWhiteSpace(tenants.TenantId) ? string.Empty : tenants.TenantId.Trim();

    private DateTime Now => time.GetUtcNow().UtcDateTime;

    private ContextOptions Token(CancellationToken cancellationToken) => ContextOptions.ForCancellationToken(cancellationToken);

    private Result RequireReader() => access.CanRead()
        ? Result.Success()
        : Result.Forbidden("You do not have permission to read the forums.");

    private Result RequireManager() => access.CanManage()
        ? Result.Success()
        : Result.Forbidden("You do not have permission to manage the forums.");

    private Result RequireModerator() => access.CanModerate()
        ? Result.Success()
        : Result.Forbidden("You do not have permission to moderate.");

    private Result<string> RequireWriter()
    {
        if (!access.CanWrite())
        {
            return Result<string>.Forbidden("Sign in to post.");
        }

        string? userId = users.GetCurrentUserId();
        return string.IsNullOrWhiteSpace(userId)
            ? Result<string>.Forbidden("Sign in to post.")
            : Result<string>.Success(userId);
    }

    private string ActorName()
    {
        string? name = users.GetCurrentUserName();
        return string.IsNullOrWhiteSpace(name) ? "Member" : name.Trim();
    }

    private static int PageOf(int pageNumber) => pageNumber < 1 ? 1 : pageNumber;

    private static string Address(string? ipAddress)
    {
        string value = ipAddress?.Trim() ?? string.Empty;
        return value.Length <= Constants.MaxLengths.IpAddress ? value : value[..Constants.MaxLengths.IpAddress];
    }

    private static Result<string> RequiredText(string? value, string field, int max, string emptyMessage)
    {
        string text = value?.Trim() ?? string.Empty;
        return text.Length == 0
            ? Result<string>.Invalid(new ValidationError(field, emptyMessage))
            : text.Length > max
            ? Result<string>.Invalid(new ValidationError(field, $"Keep it to {max} characters."))
            : Result<string>.Success(text);
    }

    private async Task<ForumPreference> PreferenceAsync(CancellationToken cancellationToken)
    {
        var existing = await preferences.FindOneAsync(new SearchOptions<ForumPreference>
        {
            Query = row => row.TenantId == Tenant,
            CancellationToken = cancellationToken,
        });

        if (existing is not null)
        {
            return existing;
        }

        try
        {
            return await preferences.InsertAsync(SeedPreference(), Token(cancellationToken));
        }
        catch (DbUpdateException)
        {
            var winner = await preferences.FindOneAsync(new SearchOptions<ForumPreference>
            {
                Query = row => row.TenantId == Tenant,
                CancellationToken = cancellationToken,
            });

            return winner ?? throw new InvalidOperationException("The forum settings row could not be created.");
        }
    }

    private ForumPreference SeedPreference()
    {
        var configured = options.Value;
        return new ForumPreference
        {
            TenantId = Tenant,
            AllowUsersToEditPosts = configured.AllowUsersToEditPosts,
            AllowUsersToDeletePosts = configured.AllowUsersToDeletePosts,
            AllowUsersToManageSubscriptions = configured.AllowUsersToManageSubscriptions,
            AllowPrivateMessages = configured.AllowPrivateMessages,
            RelativeDates = configured.RelativeDates,
            FeedsEnabled = configured.FeedsEnabled,
            SignaturesEnabled = configured.SignaturesEnabled,
            TopicsPageSize = Clamp(configured.TopicsPageSize, 1, 100),
            PostsPageSize = Clamp(configured.PostsPageSize, 1, 100),
            SearchPageSize = Clamp(configured.SearchPageSize, 1, 100),
            ActivePageSize = Clamp(configured.ActivePageSize, 1, 100),
            MessagesPageSize = Clamp(configured.MessagesPageSize, 1, 100),
            SubscriptionsPageSize = Clamp(configured.SubscriptionsPageSize, 1, 100),
            SubjectMaxLength = Clamp(configured.SubjectMaxLength, 10, Constants.MaxLengths.Subject),
            PostMaxLength = Clamp(configured.PostMaxLength, 20, 100_000),
            MessageSubjectMaxLength = Clamp(configured.MessageSubjectMaxLength, 10, Constants.MaxLengths.MessageSubject),
            MessageTextMaxLength = Clamp(configured.MessageTextMaxLength, 20, 100_000),
            SearchMinimumLength = Clamp(configured.SearchMinimumLength, 1, 20),
            FeedCount = Clamp(configured.FeedCount, 1, 100),
        };
    }

    private static ForumSettingsModel ToSettings(ForumPreference preference) => new(
        preference.AllowUsersToEditPosts,
        preference.AllowUsersToDeletePosts,
        preference.AllowUsersToManageSubscriptions,
        preference.AllowPrivateMessages,
        preference.RelativeDates,
        preference.FeedsEnabled,
        preference.SignaturesEnabled,
        preference.TopicsPageSize,
        preference.PostsPageSize,
        preference.SearchPageSize,
        preference.ActivePageSize,
        preference.MessagesPageSize,
        preference.SubscriptionsPageSize,
        preference.SubjectMaxLength,
        preference.PostMaxLength,
        preference.MessageSubjectMaxLength,
        preference.MessageTextMaxLength,
        preference.SearchMinimumLength,
        preference.FeedCount);

    private static int Clamp(int value, int min, int max) => Math.Clamp(value, min, max);

    private async Task<Result<(Forum Forum, ForumGroup Group)>> FindForumAsync(int forumId, CancellationToken cancellationToken)
    {
        var forum = await forums.FindOneAsync(new SearchOptions<Forum>
        {
            Query = item => item.Id == forumId,
            CancellationToken = cancellationToken,
        });

        if (forum is null)
        {
            return Result<(Forum, ForumGroup)>.NotFound("That forum was not found.");
        }

        var group = await groups.FindOneAsync(new SearchOptions<ForumGroup>
        {
            Query = item => item.Id == forum.ForumGroupId && item.TenantId == Tenant,
            CancellationToken = cancellationToken,
        });

        return group is null
            ? Result<(Forum, ForumGroup)>.NotFound("That forum was not found.")
            : Result<(Forum, ForumGroup)>.Success((forum, group));
    }

    private async Task<Result<(ForumTopic Topic, Forum Forum, ForumGroup Group)>> FindTopicAsync(int topicId, CancellationToken cancellationToken)
    {
        var topic = await topics.FindOneAsync(new SearchOptions<ForumTopic>
        {
            Query = item => item.Id == topicId,
            CancellationToken = cancellationToken,
        });

        if (topic is null)
        {
            return Result<(ForumTopic, Forum, ForumGroup)>.NotFound("That topic was not found.");
        }

        var forum = await FindForumAsync(topic.ForumId, cancellationToken);
        return forum.IsSuccess
            ? Result<(ForumTopic, Forum, ForumGroup)>.Success((topic, forum.Value.Forum, forum.Value.Group))
            : forum.To<(ForumTopic, Forum, ForumGroup)>();
    }

    private async Task<IReadOnlyDictionary<string, ForumMember>> MembersAsync(IEnumerable<string?> userIds, CancellationToken cancellationToken)
    {
        var ids = userIds.Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => id!).Distinct(StringComparer.Ordinal).ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<string, ForumMember>(StringComparer.Ordinal);
        }

        var found = await members.FindAsync(new SearchOptions<ForumMember>
        {
            Query = member => member.TenantId == Tenant && ids.Contains(member.UserId),
            CancellationToken = cancellationToken,
        });

        return found.ToDictionary(member => member.UserId, StringComparer.Ordinal);
    }

    private static string NameOf(IReadOnlyDictionary<string, ForumMember> membersById, string? userId) =>
        userId is not null && membersById.TryGetValue(userId, out var member) && member.DisplayName.Length > 0
            ? member.DisplayName
            : "Member";

    private TopicRowModel ToRow(ForumTopic topic, string forumName, IReadOnlyDictionary<string, ForumMember> names) =>
        new(
            topic.Id,
            topic.ForumId,
            forumName,
            topic.Subject,
            topic.TopicType,
            topic.NumReplies,
            topic.Views,
            NameOf(names, topic.UserId),
            string.IsNullOrEmpty(topic.LastPostUserId) ? null : NameOf(names, topic.LastPostUserId),
            topic.LastPostTime);

    private async Task TouchMemberAsync(string userId, string? displayName, int postDelta, CancellationToken cancellationToken)
    {
        var member = await members.FindOneAsync(new SearchOptions<ForumMember>
        {
            Query = item => item.TenantId == Tenant && item.UserId == userId,
            CancellationToken = cancellationToken,
        });

        if (member is null)
        {
            if (postDelta < 0 && string.IsNullOrWhiteSpace(displayName))
            {
                return;
            }

            await members.InsertAsync(new ForumMember
            {
                TenantId = Tenant,
                UserId = userId,
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Member" : TrimTo(displayName, Constants.MaxLengths.DisplayName),
                PostCount = Math.Max(0, postDelta),
                CreatedOnUtc = Now,
                UpdatedOnUtc = Now,
            }, Token(cancellationToken));
            return;
        }

        if (!string.IsNullOrWhiteSpace(displayName))
        {
            member.DisplayName = TrimTo(displayName, Constants.MaxLengths.DisplayName);
        }

        member.PostCount = Math.Max(0, member.PostCount + postDelta);
        member.UpdatedOnUtc = Now;
        await members.UpdateAsync(member, Token(cancellationToken));
    }

    private static string TrimTo(string value, int max)
    {
        string trimmed = value.Trim();
        return trimmed.Length <= max ? trimmed : trimmed[..max];
    }

    private bool CanEdit(ForumPost post, ForumPreference preference) =>
        access.CanModerate()
        || (access.CanWrite()
            && preference.AllowUsersToEditPosts
            && string.Equals(users.GetCurrentUserId(), post.UserId, StringComparison.Ordinal));

    private bool CanDelete(ForumPost post, ForumPreference preference) =>
        access.CanModerate()
        || (access.CanWrite()
            && preference.AllowUsersToDeletePosts
            && string.Equals(users.GetCurrentUserId(), post.UserId, StringComparison.Ordinal));

    private bool CanSubscribe(ForumPreference preference) =>
        access.CanWrite() && (preference.AllowUsersToManageSubscriptions || access.CanModerate());

    private async Task<ForumSubscription?> FindSubscriptionAsync(string userId, int forumId, int topicId, CancellationToken cancellationToken) =>
        await subscriptions.FindOneAsync(new SearchOptions<ForumSubscription>
        {
            Query = item => item.TenantId == Tenant && item.UserId == userId && item.ForumId == forumId && item.TopicId == topicId,
            CancellationToken = cancellationToken,
        });

    private async Task RecalculateForumAsync(int forumId, CancellationToken cancellationToken)
    {
        var forum = await forums.FindOneAsync(forumId);
        if (forum is null)
        {
            return;
        }

        using var connection = topics.OpenConnection(Token(cancellationToken));
        var query = connection.Query(topic => topic.ForumId == forumId);
        int topicCount = await query.CountAsync(cancellationToken);
        int postCount = await query.SumAsync(topic => (int?)topic.NumPosts, cancellationToken) ?? 0;
        var last = await query
            .OrderByDescending(topic => topic.LastPostTime)
            .ThenByDescending(topic => topic.Id)
            .Select(topic => new { topic.Id, topic.LastPostId, topic.LastPostUserId, topic.LastPostTime })
            .FirstOrDefaultAsync(cancellationToken);

        forum.NumTopics = topicCount;
        forum.NumPosts = postCount;
        forum.LastTopicId = last?.Id ?? 0;
        forum.LastPostId = last?.LastPostId ?? 0;
        forum.LastPostUserId = last?.LastPostUserId ?? string.Empty;
        forum.LastPostTime = last?.LastPostTime;
        forum.UpdatedOnUtc = Now;
        await forums.UpdateAsync(forum, Token(cancellationToken));
    }

    private async Task NotifyAsync(Forum forum, ForumTopic topic, ForumPost post, string authorId, CancellationToken cancellationToken)
    {
        var subs = await subscriptions.FindAsync(new SearchOptions<ForumSubscription>
        {
            Query = item => item.TenantId == Tenant
                && item.UserId != authorId
                && ((item.ForumId == forum.Id && item.TopicId == 0) || item.TopicId == topic.Id),
            CancellationToken = cancellationToken,
        });

        var notice = new ForumPostNotice(
            forum.Id,
            forum.Name,
            topic.Id,
            topic.Subject,
            post.Id,
            authorId,
            ActorName(),
            subs.Select(item => item.UserId).Distinct(StringComparer.Ordinal).ToList());

        try
        {
            await notifier.NotifyAsync(notice, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Forum notification failed for topic {TopicId}", topic.Id);
        }
    }
}