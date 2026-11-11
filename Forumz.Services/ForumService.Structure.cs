namespace Forumz.Services;

public sealed partial class ForumService
{
    /// <inheritdoc />
    public async Task<Result<ForumStructureModel>> GetStructureAsync(CancellationToken cancellationToken = default)
    {
        var manager = RequireManager();
        if (!manager.IsSuccess)
        {
            return manager.To<ForumStructureModel>();
        }

        var index = await GetIndexAsync(cancellationToken);
        if (!index.IsSuccess)
        {
            return index.To<ForumStructureModel>();
        }

        var editGroups = new List<ForumGroupEditModel>();
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

        var byGroup = (forumPage ?? Enumerable.Empty<Forum>()).GroupBy(forum => forum.ForumGroupId).ToDictionary(group => group.Key, group => group.ToList());
        foreach (var group in groupPage)
        {
            var rows = byGroup.TryGetValue(group.Id, out var found) ? found : [];
            editGroups.Add(new ForumGroupEditModel(
                group.Id,
                group.Name,
                group.DisplayOrder,
                rows.Select(forum => new ForumEditModel(forum.Id, forum.ForumGroupId, forum.Name, forum.Description, forum.DisplayOrder)).ToList()));
        }

        return Result<ForumStructureModel>.Success(new ForumStructureModel(editGroups));
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<ForumChoiceModel>>> GetForumChoicesAsync(CancellationToken cancellationToken = default)
    {
        var reader = RequireReader();
        if (!reader.IsSuccess)
        {
            return reader.To<IReadOnlyList<ForumChoiceModel>>();
        }

        var structure = await groups.FindAsync(new SearchOptions<ForumGroup>
        {
            Query = group => group.TenantId == Tenant,
            OrderBy = query => query.OrderBy(group => group.DisplayOrder).ThenBy(group => group.Name),
            CancellationToken = cancellationToken,
        });

        var groupIds = structure.Select(group => group.Id).ToList();
        var forumPage = groupIds.Count == 0
            ? null
            : await forums.FindAsync(new SearchOptions<Forum>
            {
                Query = forum => groupIds.Contains(forum.ForumGroupId),
                OrderBy = query => query.OrderBy(forum => forum.DisplayOrder).ThenBy(forum => forum.Name),
                CancellationToken = cancellationToken,
            });

        var names = structure.ToDictionary(group => group.Id, group => group.Name);
        IReadOnlyList<ForumChoiceModel> choices = (forumPage ?? Enumerable.Empty<Forum>())
            .Select(forum => new ForumChoiceModel(forum.Id, names.GetValueOrDefault(forum.ForumGroupId, "Group"), forum.Name))
            .ToList();
        return Result<IReadOnlyList<ForumChoiceModel>>.Success(choices);
    }

    /// <inheritdoc />
    public async Task<Result<int>> SaveGroupAsync(SaveForumGroupRequest request, CancellationToken cancellationToken = default)
    {
        var manager = RequireManager();
        if (!manager.IsSuccess)
        {
            return manager.To<int>();
        }

        if (request is null)
        {
            return Result<int>.Invalid(new ValidationError(nameof(request), "A group is required."));
        }

        var name = RequiredText(request.Name, nameof(request.Name), Constants.MaxLengths.Name, "Give the group a name.");
        if (!name.IsSuccess)
        {
            return name.To<int>();
        }

        if (request.Id is null or 0)
        {
            var created = await groups.InsertAsync(new ForumGroup
            {
                TenantId = Tenant,
                Name = name.Value,
                DisplayOrder = request.DisplayOrder,
                CreatedOnUtc = Now,
                UpdatedOnUtc = Now,
            }, Token(cancellationToken));
            logger.LogInformation("User {UserId} created forum group {GroupId}", users.GetCurrentUserId(), created.Id);
            return Result<int>.Success(created.Id);
        }

        var group = await groups.FindOneAsync(new SearchOptions<ForumGroup>
        {
            Query = item => item.Id == request.Id && item.TenantId == Tenant,
            CancellationToken = cancellationToken,
        });

        if (group is null)
        {
            return Result<int>.NotFound("That group was not found.");
        }

        group.Name = name.Value;
        group.DisplayOrder = request.DisplayOrder;
        group.UpdatedOnUtc = Now;
        await groups.UpdateAsync(group, Token(cancellationToken));
        logger.LogInformation("User {UserId} updated forum group {GroupId}", users.GetCurrentUserId(), group.Id);
        return Result<int>.Success(group.Id);
    }

    /// <inheritdoc />
    public async Task<Result> DeleteGroupAsync(int groupId, CancellationToken cancellationToken = default)
    {
        var manager = RequireManager();
        if (!manager.IsSuccess)
        {
            return manager;
        }

        var group = await groups.FindOneAsync(new SearchOptions<ForumGroup>
        {
            Query = item => item.Id == groupId && item.TenantId == Tenant,
            CancellationToken = cancellationToken,
        });

        if (group is null)
        {
            return Result.NotFound("That group was not found.");
        }

        var children = await forums.FindAsync(new SearchOptions<Forum>
        {
            Query = forum => forum.ForumGroupId == groupId,
            CancellationToken = cancellationToken,
        });

        if (children.Count > 0)
        {
            return Result.Invalid(new ValidationError(nameof(groupId), "Delete the forums in this group first."));
        }

        await groups.DeleteAsync(group, Token(cancellationToken));
        logger.LogInformation("User {UserId} deleted forum group {GroupId}", users.GetCurrentUserId(), groupId);
        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result<int>> SaveForumAsync(SaveForumRequest request, CancellationToken cancellationToken = default)
    {
        var manager = RequireManager();
        if (!manager.IsSuccess)
        {
            return manager.To<int>();
        }

        if (request is null)
        {
            return Result<int>.Invalid(new ValidationError(nameof(request), "A forum is required."));
        }

        var group = await groups.FindOneAsync(new SearchOptions<ForumGroup>
        {
            Query = item => item.Id == request.GroupId && item.TenantId == Tenant,
            CancellationToken = cancellationToken,
        });

        if (group is null)
        {
            return Result<int>.NotFound("That group was not found.");
        }

        var name = RequiredText(request.Name, nameof(request.Name), Constants.MaxLengths.Name, "Give the forum a name.");
        if (!name.IsSuccess)
        {
            return name.To<int>();
        }

        string description = request.Description?.Trim() ?? string.Empty;
        if (description.Length > Constants.MaxLengths.Description)
        {
            return Result<int>.Invalid(new ValidationError(nameof(request.Description), $"Keep the description to {Constants.MaxLengths.Description} characters."));
        }

        if (request.Id is null or 0)
        {
            var created = await forums.InsertAsync(new Forum
            {
                ForumGroupId = group.Id,
                Name = name.Value,
                Description = description,
                DisplayOrder = request.DisplayOrder,
                CreatedOnUtc = Now,
                UpdatedOnUtc = Now,
            }, Token(cancellationToken));
            logger.LogInformation("User {UserId} created forum {ForumId}", users.GetCurrentUserId(), created.Id);
            return Result<int>.Success(created.Id);
        }

        var forum = await forums.FindOneAsync(request.Id.Value);
        if (forum is null || (forum.ForumGroupId != group.Id && !await GroupInTenantAsync(forum.ForumGroupId, cancellationToken)))
        {
            return Result<int>.NotFound("That forum was not found.");
        }

        var owning = await groups.FindOneAsync(new SearchOptions<ForumGroup>
        {
            Query = item => item.Id == forum.ForumGroupId && item.TenantId == Tenant,
            CancellationToken = cancellationToken,
        });

        if (owning is null)
        {
            return Result<int>.NotFound("That forum was not found.");
        }

        forum.ForumGroupId = group.Id;
        forum.Name = name.Value;
        forum.Description = description;
        forum.DisplayOrder = request.DisplayOrder;
        forum.UpdatedOnUtc = Now;
        await forums.UpdateAsync(forum, Token(cancellationToken));
        logger.LogInformation("User {UserId} updated forum {ForumId}", users.GetCurrentUserId(), forum.Id);
        return Result<int>.Success(forum.Id);
    }

    /// <inheritdoc />
    public async Task<Result> DeleteForumAsync(int forumId, CancellationToken cancellationToken = default)
    {
        var manager = RequireManager();
        if (!manager.IsSuccess)
        {
            return manager;
        }

        var found = await FindForumAsync(forumId, cancellationToken);
        if (!found.IsSuccess)
        {
            return found.ToVoid();
        }

        var topicPage = await topics.FindAsync(new SearchOptions<ForumTopic>
        {
            Query = topic => topic.ForumId == forumId,
            CancellationToken = cancellationToken,
        });
        var topicIds = topicPage.Select(topic => topic.Id).ToList();

        if (topicIds.Count > 0)
        {
            var authored = await posts.FindAsync(new SearchOptions<ForumPost>
            {
                Query = post => topicIds.Contains(post.TopicId),
                CancellationToken = cancellationToken,
            });

            foreach (var group in authored.GroupBy(post => post.UserId))
            {
                await TouchMemberAsync(group.Key, null, -group.Count(), cancellationToken);
            }

            await posts.DeleteAsync(post => topicIds.Contains(post.TopicId), Token(cancellationToken));
            await subscriptions.DeleteAsync(item => item.TenantId == Tenant && topicIds.Contains(item.TopicId), Token(cancellationToken));
            await topics.DeleteAsync(topic => topic.ForumId == forumId, Token(cancellationToken));
        }

        await subscriptions.DeleteAsync(item => item.TenantId == Tenant && item.ForumId == forumId, Token(cancellationToken));
        await forums.DeleteAsync(found.Value.Forum, Token(cancellationToken));
        logger.LogInformation("User {UserId} deleted forum {ForumId}", users.GetCurrentUserId(), forumId);
        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result<ForumSettingsModel>> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var reader = RequireReader();
        return !reader.IsSuccess
            ? reader.To<ForumSettingsModel>()
            : Result<ForumSettingsModel>.Success(ToSettings(await PreferenceAsync(cancellationToken)));
    }

    /// <inheritdoc />
    public async Task<Result> SaveSettingsAsync(SaveForumSettingsRequest request, CancellationToken cancellationToken = default)
    {
        var manager = RequireManager();
        if (!manager.IsSuccess)
        {
            return manager;
        }

        if (request is null)
        {
            return Result.Invalid(new ValidationError(nameof(request), "Settings are required."));
        }

        var preference = await PreferenceAsync(cancellationToken);
        preference.AllowUsersToEditPosts = request.AllowUsersToEditPosts;
        preference.AllowUsersToDeletePosts = request.AllowUsersToDeletePosts;
        preference.AllowUsersToManageSubscriptions = request.AllowUsersToManageSubscriptions;
        preference.AllowPrivateMessages = request.AllowPrivateMessages;
        preference.RelativeDates = request.RelativeDates;
        preference.FeedsEnabled = request.FeedsEnabled;
        preference.SignaturesEnabled = request.SignaturesEnabled;
        preference.TopicsPageSize = Clamp(request.TopicsPageSize, 1, 100);
        preference.PostsPageSize = Clamp(request.PostsPageSize, 1, 100);
        preference.SearchPageSize = Clamp(request.SearchPageSize, 1, 100);
        preference.ActivePageSize = Clamp(request.ActivePageSize, 1, 100);
        preference.MessagesPageSize = Clamp(request.MessagesPageSize, 1, 100);
        preference.SubscriptionsPageSize = Clamp(request.SubscriptionsPageSize, 1, 100);
        preference.SubjectMaxLength = Clamp(request.SubjectMaxLength, 10, Constants.MaxLengths.Subject);
        preference.PostMaxLength = Clamp(request.PostMaxLength, 20, 100_000);
        preference.MessageSubjectMaxLength = Clamp(request.MessageSubjectMaxLength, 10, Constants.MaxLengths.MessageSubject);
        preference.MessageTextMaxLength = Clamp(request.MessageTextMaxLength, 20, 100_000);
        preference.SearchMinimumLength = Clamp(request.SearchMinimumLength, 1, 20);
        preference.FeedCount = Clamp(request.FeedCount, 1, 100);
        await preferences.UpdateAsync(preference, Token(cancellationToken));
        logger.LogInformation("User {UserId} updated forum settings for tenant {TenantId}", users.GetCurrentUserId(), Tenant);
        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result<MemberProfileModel>> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        var writer = RequireWriter();
        if (!writer.IsSuccess)
        {
            return writer.To<MemberProfileModel>();
        }

        await TouchMemberAsync(writer.Value, ActorName(), 0, cancellationToken);
        var member = await members.FindOneAsync(new SearchOptions<ForumMember>
        {
            Query = item => item.TenantId == Tenant && item.UserId == writer.Value,
            CancellationToken = cancellationToken,
        });

        return member is null
            ? Result<MemberProfileModel>.NotFound("Your profile was not found.")
            : Result<MemberProfileModel>.Success(new MemberProfileModel(member.DisplayName, member.Signature, member.PostCount));
    }

    /// <inheritdoc />
    public async Task<Result> SaveProfileAsync(SaveProfileRequest request, CancellationToken cancellationToken = default)
    {
        var writer = RequireWriter();
        if (!writer.IsSuccess)
        {
            return writer.ToVoid();
        }

        if (request is null)
        {
            return Result.Invalid(new ValidationError(nameof(request), "A profile is required."));
        }

        var name = RequiredText(request.DisplayName, nameof(request.DisplayName), Constants.MaxLengths.DisplayName, "Give yourself a name.");
        if (!name.IsSuccess)
        {
            return name.ToVoid();
        }

        string signature = request.Signature?.Trim() ?? string.Empty;
        if (signature.Length > Constants.MaxLengths.Signature)
        {
            return Result.Invalid(new ValidationError(nameof(request.Signature), $"Keep the signature to {Constants.MaxLengths.Signature} characters."));
        }

        await TouchMemberAsync(writer.Value, name.Value, 0, cancellationToken);
        var member = await members.FindOneAsync(new SearchOptions<ForumMember>
        {
            Query = item => item.TenantId == Tenant && item.UserId == writer.Value,
            CancellationToken = cancellationToken,
        });

        if (member is null)
        {
            return Result.NotFound("Your profile was not found.");
        }

        member.Signature = signature;
        member.UpdatedOnUtc = Now;
        await members.UpdateAsync(member, Token(cancellationToken));
        logger.LogInformation("User {UserId} updated their forum profile", writer.Value);
        return Result.Success();
    }

    private async Task<bool> GroupInTenantAsync(int groupId, CancellationToken cancellationToken)
    {
        var group = await groups.FindOneAsync(new SearchOptions<ForumGroup>
        {
            Query = item => item.Id == groupId && item.TenantId == Tenant,
            CancellationToken = cancellationToken,
        });
        return group is not null;
    }
}