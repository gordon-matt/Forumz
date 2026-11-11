namespace Forumz.Services;

public sealed partial class ForumService
{
    /// <inheritdoc />
    public async Task<Result<int>> CreateTopicAsync(CreateTopicRequest request, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var writer = RequireWriter();
        if (!writer.IsSuccess)
        {
            return writer.To<int>();
        }

        if (request is null)
        {
            return Result<int>.Invalid(new ValidationError(nameof(request), "A topic is required."));
        }

        var found = await FindForumAsync(request.ForumId, cancellationToken);
        if (!found.IsSuccess)
        {
            return found.To<int>();
        }

        var preference = await PreferenceAsync(cancellationToken);
        var subject = RequiredText(request.Subject, nameof(request.Subject), preference.SubjectMaxLength, "Give the topic a subject.");
        if (!subject.IsSuccess)
        {
            return subject.To<int>();
        }

        var text = RequiredText(request.Text, nameof(request.Text), preference.PostMaxLength, "Write the first post.");
        if (!text.IsSuccess)
        {
            return text.To<int>();
        }

        string userId = writer.Value;
        var now = Now;
        var topicType = access.CanModerate() ? request.TopicType : ForumTopicType.Normal;
        var topic = await topics.InsertAsync(new ForumTopic
        {
            ForumId = request.ForumId,
            UserId = userId,
            TopicType = topicType,
            Subject = subject.Value,
            CreatedOnUtc = now,
            UpdatedOnUtc = now,
        }, Token(cancellationToken));

        var post = await posts.InsertAsync(new ForumPost
        {
            TopicId = topic.Id,
            UserId = userId,
            Text = text.Value,
            Format = request.Format,
            IpAddress = Address(ipAddress),
            CreatedOnUtc = now,
            UpdatedOnUtc = now,
        }, Token(cancellationToken));

        topic.NumPosts = 1;
        topic.LastPostId = post.Id;
        topic.LastPostUserId = userId;
        topic.LastPostTime = now;
        await topics.UpdateAsync(topic, Token(cancellationToken));

        var forum = found.Value.Forum;
        forum.NumTopics += 1;
        forum.NumPosts += 1;
        forum.LastTopicId = topic.Id;
        forum.LastPostId = post.Id;
        forum.LastPostUserId = userId;
        forum.LastPostTime = now;
        forum.UpdatedOnUtc = now;
        await forums.UpdateAsync(forum, Token(cancellationToken));
        await TouchMemberAsync(userId, ActorName(), 1, cancellationToken);
        await NotifyAsync(forum, topic, post, userId, cancellationToken);

        logger.LogInformation("User {UserId} started topic {TopicId} in forum {ForumId}", userId, topic.Id, forum.Id);
        return Result<int>.Success(topic.Id);
    }

    /// <inheritdoc />
    public async Task<Result<int>> CreatePostAsync(CreatePostRequest request, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var writer = RequireWriter();
        if (!writer.IsSuccess)
        {
            return writer.To<int>();
        }

        if (request is null)
        {
            return Result<int>.Invalid(new ValidationError(nameof(request), "A reply is required."));
        }

        var found = await FindTopicAsync(request.TopicId, cancellationToken);
        if (!found.IsSuccess)
        {
            return found.To<int>();
        }

        var preference = await PreferenceAsync(cancellationToken);
        var text = RequiredText(request.Text, nameof(request.Text), preference.PostMaxLength, "Write a reply.");
        if (!text.IsSuccess)
        {
            return text.To<int>();
        }

        string userId = writer.Value;
        var now = Now;
        var (topic, forum, _) = found.Value;
        var post = await posts.InsertAsync(new ForumPost
        {
            TopicId = topic.Id,
            UserId = userId,
            Text = text.Value,
            Format = request.Format,
            IpAddress = Address(ipAddress),
            CreatedOnUtc = now,
            UpdatedOnUtc = now,
        }, Token(cancellationToken));

        topic.NumPosts += 1;
        topic.LastPostId = post.Id;
        topic.LastPostUserId = userId;
        topic.LastPostTime = now;
        topic.UpdatedOnUtc = now;
        await topics.UpdateAsync(topic, Token(cancellationToken));

        forum.NumPosts += 1;
        forum.LastTopicId = topic.Id;
        forum.LastPostId = post.Id;
        forum.LastPostUserId = userId;
        forum.LastPostTime = now;
        forum.UpdatedOnUtc = now;
        await forums.UpdateAsync(forum, Token(cancellationToken));
        await TouchMemberAsync(userId, ActorName(), 1, cancellationToken);
        await NotifyAsync(forum, topic, post, userId, cancellationToken);

        logger.LogInformation("User {UserId} replied to topic {TopicId} with post {PostId}", userId, topic.Id, post.Id);
        return Result<int>.Success(post.Id);
    }

    /// <inheritdoc />
    public async Task<Result> UpdatePostAsync(UpdatePostRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return Result.Invalid(new ValidationError(nameof(request), "A post is required."));
        }

        var post = await posts.FindOneAsync(request.PostId);
        if (post is null)
        {
            return Result.NotFound("That post was not found.");
        }

        var found = await FindTopicAsync(post.TopicId, cancellationToken);
        if (!found.IsSuccess)
        {
            return found.ToVoid();
        }

        var preference = await PreferenceAsync(cancellationToken);
        if (!CanEdit(post, preference))
        {
            return Result.Forbidden("You cannot edit that post.");
        }

        var text = RequiredText(request.Text, nameof(request.Text), preference.PostMaxLength, "Write something.");
        if (!text.IsSuccess)
        {
            return text.ToVoid();
        }

        post.Text = text.Value;
        post.Format = request.Format;
        post.UpdatedOnUtc = Now;
        await posts.UpdateAsync(post, Token(cancellationToken));
        logger.LogInformation("User {UserId} edited post {PostId}", users.GetCurrentUserId(), post.Id);
        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> DeletePostAsync(int postId, CancellationToken cancellationToken = default)
    {
        var post = await posts.FindOneAsync(postId);
        if (post is null)
        {
            return Result.NotFound("That post was not found.");
        }

        var found = await FindTopicAsync(post.TopicId, cancellationToken);
        if (!found.IsSuccess)
        {
            return found.ToVoid();
        }

        var preference = await PreferenceAsync(cancellationToken);
        if (!CanDelete(post, preference))
        {
            return Result.Forbidden("You cannot delete that post.");
        }

        var (topic, forum, _) = found.Value;
        if (topic.NumPosts <= 1)
        {
            return await DeleteTopicAsync(topic.Id, cancellationToken);
        }

        await posts.DeleteAsync(post, Token(cancellationToken));
        await TouchMemberAsync(post.UserId, null, -1, cancellationToken);

        var last = await posts.FindOneAsync(new SearchOptions<ForumPost>
        {
            Query = item => item.TopicId == topic.Id,
            OrderBy = query => query.OrderByDescending(item => item.CreatedOnUtc).ThenByDescending(item => item.Id),
            CancellationToken = cancellationToken,
        });

        topic.NumPosts = Math.Max(0, topic.NumPosts - 1);
        topic.LastPostId = last?.Id ?? 0;
        topic.LastPostUserId = last?.UserId ?? string.Empty;
        topic.LastPostTime = last?.CreatedOnUtc;
        topic.UpdatedOnUtc = Now;
        await topics.UpdateAsync(topic, Token(cancellationToken));
        await RecalculateForumAsync(forum.Id, cancellationToken);

        logger.LogInformation("User {UserId} deleted post {PostId}", users.GetCurrentUserId(), postId);
        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> DeleteTopicAsync(int topicId, CancellationToken cancellationToken = default)
    {
        var found = await FindTopicAsync(topicId, cancellationToken);
        if (!found.IsSuccess)
        {
            return found.ToVoid();
        }

        var (topic, forum, _) = found.Value;
        var preference = await PreferenceAsync(cancellationToken);
        var opening = await posts.FindOneAsync(new SearchOptions<ForumPost>
        {
            Query = post => post.TopicId == topicId,
            OrderBy = query => query.OrderBy(post => post.CreatedOnUtc).ThenBy(post => post.Id),
            CancellationToken = cancellationToken,
        });

        if (opening is not null && !CanDelete(opening, preference) && !access.CanModerate())
        {
            return Result.Forbidden("You cannot delete that topic.");
        }

        if (opening is null && !access.CanModerate())
        {
            return Result.Forbidden("You cannot delete that topic.");
        }

        var authored = await posts.FindAsync(new SearchOptions<ForumPost>
        {
            Query = post => post.TopicId == topicId,
            CancellationToken = cancellationToken,
        });

        foreach (var group in authored.GroupBy(post => post.UserId))
        {
            await TouchMemberAsync(group.Key, null, -group.Count(), cancellationToken);
        }

        await posts.DeleteAsync(post => post.TopicId == topicId, Token(cancellationToken));
        await subscriptions.DeleteAsync(item => item.TenantId == Tenant && item.TopicId == topicId, Token(cancellationToken));
        await topics.DeleteAsync(topic, Token(cancellationToken));
        await RecalculateForumAsync(forum.Id, cancellationToken);

        logger.LogInformation("User {UserId} deleted topic {TopicId}", users.GetCurrentUserId(), topicId);
        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> MoveTopicAsync(int topicId, int destinationForumId, CancellationToken cancellationToken = default)
    {
        var moderator = RequireModerator();
        if (!moderator.IsSuccess)
        {
            return moderator;
        }

        var found = await FindTopicAsync(topicId, cancellationToken);
        if (!found.IsSuccess)
        {
            return found.ToVoid();
        }

        var destination = await FindForumAsync(destinationForumId, cancellationToken);
        if (!destination.IsSuccess)
        {
            return destination.ToVoid();
        }

        var (topic, forum, _) = found.Value;
        if (forum.Id == destinationForumId)
        {
            return Result.Success();
        }

        int previousForumId = forum.Id;
        topic.ForumId = destinationForumId;
        topic.UpdatedOnUtc = Now;
        await topics.UpdateAsync(topic, Token(cancellationToken));
        await subscriptions.UpdateAsync(
            item => item.TenantId == Tenant && item.TopicId == topicId,
            setters => setters.SetProperty(item => item.ForumId, destinationForumId),
            Token(cancellationToken));
        await RecalculateForumAsync(previousForumId, cancellationToken);
        await RecalculateForumAsync(destinationForumId, cancellationToken);

        logger.LogInformation(
            "User {UserId} moved topic {TopicId} from forum {FromForumId} to forum {ToForumId}",
            users.GetCurrentUserId(),
            topicId,
            previousForumId,
            destinationForumId);
        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> SetTopicTypeAsync(int topicId, ForumTopicType topicType, CancellationToken cancellationToken = default)
    {
        var moderator = RequireModerator();
        if (!moderator.IsSuccess)
        {
            return moderator;
        }

        if (!Enum.IsDefined(topicType))
        {
            return Result.Invalid(new ValidationError(nameof(topicType), "Choose a topic type."));
        }

        var found = await FindTopicAsync(topicId, cancellationToken);
        if (!found.IsSuccess)
        {
            return found.ToVoid();
        }

        var topic = found.Value.Topic;
        topic.TopicType = topicType;
        topic.UpdatedOnUtc = Now;
        await topics.UpdateAsync(topic, Token(cancellationToken));
        logger.LogInformation("User {UserId} set topic {TopicId} to {TopicType}", users.GetCurrentUserId(), topicId, topicType);
        return Result.Success();
    }
}