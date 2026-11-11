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
        if (!found.IsSuccess)
        {
            return found.ToVoid();
        }

        return await SubscribeAsync(found.Value.Forum.Id, topicId, cancellationToken);
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

    /// <inheritdoc />
    public Task<Result<MessageListModel>> GetInboxAsync(int pageNumber, CancellationToken cancellationToken = default) =>
        ListMessagesAsync(incoming: true, pageNumber, cancellationToken);

    /// <inheritdoc />
    public Task<Result<MessageListModel>> GetSentAsync(int pageNumber, CancellationToken cancellationToken = default) =>
        ListMessagesAsync(incoming: false, pageNumber, cancellationToken);

    private async Task<Result<MessageListModel>> ListMessagesAsync(bool incoming, int pageNumber, CancellationToken cancellationToken)
    {
        var writer = RequireWriter();
        if (!writer.IsSuccess)
        {
            return writer.To<MessageListModel>();
        }

        var preference = await PreferenceAsync(cancellationToken);
        if (!preference.AllowPrivateMessages)
        {
            return Result<MessageListModel>.Forbidden("Private messages are turned off.");
        }

        string userId = writer.Value;
        int page = PageOf(pageNumber);
        var pageItems = await messages.FindAsync(new SearchOptions<PrivateMessage>
        {
            Query = incoming
                ? item => item.TenantId == Tenant && item.ToUserId == userId && !item.IsDeletedByRecipient
                : item => item.TenantId == Tenant && item.FromUserId == userId && !item.IsDeletedByAuthor,
            OrderBy = query => query.OrderByDescending(item => item.CreatedOnUtc),
            PageNumber = page,
            PageSize = preference.MessagesPageSize,
            CancellationToken = cancellationToken,
        });

        var unread = await messages.FindAsync(new SearchOptions<PrivateMessage>
        {
            Query = item => item.TenantId == Tenant && item.ToUserId == userId && !item.IsDeletedByRecipient && !item.IsRead,
            CancellationToken = cancellationToken,
        });

        var otherIds = pageItems.Select(item => incoming ? item.FromUserId : item.ToUserId);
        var names = await MembersAsync(otherIds, cancellationToken);
        var rows = pageItems.Select(item =>
        {
            string other = incoming ? item.FromUserId : item.ToUserId;
            return new MessageRowModel(item.Id, other, NameOf(names, other), item.Subject, item.IsRead, item.CreatedOnUtc);
        }).ToList();

        return Result<MessageListModel>.Success(new MessageListModel(rows, page, preference.MessagesPageSize, pageItems.ItemCount, unread.ItemCount));
    }

    /// <inheritdoc />
    public async Task<Result<PrivateMessageModel>> GetMessageAsync(int messageId, CancellationToken cancellationToken = default)
    {
        var writer = RequireWriter();
        if (!writer.IsSuccess)
        {
            return writer.To<PrivateMessageModel>();
        }

        var preference = await PreferenceAsync(cancellationToken);
        if (!preference.AllowPrivateMessages)
        {
            return Result<PrivateMessageModel>.Forbidden("Private messages are turned off.");
        }

        string userId = writer.Value;
        var message = await messages.FindOneAsync(new SearchOptions<PrivateMessage>
        {
            Query = item => item.Id == messageId && item.TenantId == Tenant,
            CancellationToken = cancellationToken,
        });

        if (message is null)
        {
            return Result<PrivateMessageModel>.NotFound("That message was not found.");
        }

        bool inbox = string.Equals(message.ToUserId, userId, StringComparison.Ordinal);
        bool sent = string.Equals(message.FromUserId, userId, StringComparison.Ordinal);
        if ((inbox && message.IsDeletedByRecipient) || (sent && !inbox && message.IsDeletedByAuthor) || (!inbox && !sent))
        {
            return Result<PrivateMessageModel>.NotFound("That message was not found.");
        }

        if (inbox && !message.IsRead)
        {
            message.IsRead = true;
            message = await messages.UpdateAsync(message, Token(cancellationToken));
        }

        var names = await MembersAsync([message.FromUserId, message.ToUserId], cancellationToken);
        return Result<PrivateMessageModel>.Success(new PrivateMessageModel(
            message.Id,
            message.FromUserId,
            NameOf(names, message.FromUserId),
            message.ToUserId,
            NameOf(names, message.ToUserId),
            message.Subject,
            message.Text,
            message.IsRead,
            message.CreatedOnUtc,
            inbox));
    }

    /// <inheritdoc />
    public async Task<Result<int>> SendMessageAsync(SendMessageRequest request, CancellationToken cancellationToken = default)
    {
        var writer = RequireWriter();
        if (!writer.IsSuccess)
        {
            return writer.To<int>();
        }

        var preference = await PreferenceAsync(cancellationToken);
        if (!preference.AllowPrivateMessages)
        {
            return Result<int>.Forbidden("Private messages are turned off.");
        }

        if (request is null)
        {
            return Result<int>.Invalid(new ValidationError(nameof(request), "A message is required."));
        }

        string to = request.To?.Trim() ?? string.Empty;
        if (to.Length == 0)
        {
            return Result<int>.Invalid(new ValidationError(nameof(request.To), "Say who the message is for."));
        }

        var subject = RequiredText(request.Subject, nameof(request.Subject), preference.MessageSubjectMaxLength, "Give the message a subject.");
        if (!subject.IsSuccess)
        {
            return subject.To<int>();
        }

        var text = RequiredText(request.Text, nameof(request.Text), preference.MessageTextMaxLength, "Write the message.");
        if (!text.IsSuccess)
        {
            return text.To<int>();
        }

        string key = to.ToLowerInvariant();
        var matches = await members.FindAsync(new SearchOptions<ForumMember>
        {
            Query = member => member.TenantId == Tenant && (member.UserId.ToLower() == key || member.DisplayName.ToLower() == key),
            CancellationToken = cancellationToken,
        });

        if (matches.Count == 0)
        {
            return Result<int>.Invalid(new ValidationError(nameof(request.To), "No member uses that name. They appear here after they post or save a profile."));
        }

        if (matches.Count > 1)
        {
            return Result<int>.Invalid(new ValidationError(nameof(request.To), "More than one member uses that name."));
        }

        var recipient = matches.Single();
        if (string.Equals(recipient.UserId, writer.Value, StringComparison.Ordinal))
        {
            return Result<int>.Invalid(new ValidationError(nameof(request.To), "Send it to someone else."));
        }

        var saved = await messages.InsertAsync(new PrivateMessage
        {
            TenantId = Tenant,
            FromUserId = writer.Value,
            ToUserId = recipient.UserId,
            Subject = subject.Value,
            Text = text.Value,
            CreatedOnUtc = Now,
        }, Token(cancellationToken));

        await TouchMemberAsync(writer.Value, ActorName(), 0, cancellationToken);
        logger.LogInformation("User {UserId} sent message {MessageId} to {RecipientId}", writer.Value, saved.Id, recipient.UserId);
        return Result<int>.Success(saved.Id);
    }

    /// <inheritdoc />
    public async Task<Result> DeleteMessageAsync(int messageId, CancellationToken cancellationToken = default)
    {
        var writer = RequireWriter();
        if (!writer.IsSuccess)
        {
            return writer.ToVoid();
        }

        string userId = writer.Value;
        var message = await messages.FindOneAsync(new SearchOptions<PrivateMessage>
        {
            Query = item => item.Id == messageId && item.TenantId == Tenant,
            CancellationToken = cancellationToken,
        });

        if (message is null)
        {
            return Result.NotFound("That message was not found.");
        }

        bool inbox = string.Equals(message.ToUserId, userId, StringComparison.Ordinal);
        bool sent = string.Equals(message.FromUserId, userId, StringComparison.Ordinal);
        if (!inbox && !sent)
        {
            return Result.NotFound("That message was not found.");
        }

        if (inbox)
        {
            message.IsDeletedByRecipient = true;
        }

        if (sent)
        {
            message.IsDeletedByAuthor = true;
        }

        if (message.IsDeletedByAuthor && message.IsDeletedByRecipient)
        {
            await messages.DeleteAsync(message, Token(cancellationToken));
        }
        else
        {
            await messages.UpdateAsync(message, Token(cancellationToken));
        }

        logger.LogInformation("User {UserId} deleted message {MessageId}", userId, messageId);
        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result<int>> GetUnreadMessageCountAsync(CancellationToken cancellationToken = default)
    {
        if (!access.CanWrite() || string.IsNullOrWhiteSpace(users.GetCurrentUserId()))
        {
            return Result<int>.Success(0);
        }

        var preference = await PreferenceAsync(cancellationToken);
        if (!preference.AllowPrivateMessages)
        {
            return Result<int>.Success(0);
        }

        string userId = users.GetCurrentUserId()!;
        var unread = await messages.FindAsync(new SearchOptions<PrivateMessage>
        {
            Query = item => item.TenantId == Tenant && item.ToUserId == userId && !item.IsDeletedByRecipient && !item.IsRead,
            CancellationToken = cancellationToken,
        });

        return Result<int>.Success(unread.ItemCount);
    }
}
