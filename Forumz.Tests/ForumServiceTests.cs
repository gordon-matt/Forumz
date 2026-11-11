using Forumz.Core;
using Forumz.Models;
using Forumz.Services;

namespace Forumz.Tests;

public sealed class ForumServiceTests : IDisposable
{
    private readonly TestForumDatabase database = new();

    public void Dispose() => database.Dispose();

    [Fact]
    public async Task CreateTopic_SignedInUser_UpdatesCounts()
    {
        var user = new TestUser();
        var service = ForumHarness.Create(database, user);
        int forumId = await CreateForumAsync();

        var created = await service.CreateTopicAsync(new CreateTopicRequest(forumId, "Hello", "First post"), "127.0.0.1");
        var reply = await service.CreatePostAsync(new CreatePostRequest(created.Value, "A reply"), null);
        var page = await service.GetForumAsync(forumId, 1);

        Assert.True(created.IsSuccess);
        Assert.True(reply.IsSuccess);
        Assert.Single(page.Value.Topics);
        Assert.Equal(1, page.Value.Topics[0].ReplyCount);
        var index = await service.GetIndexAsync();
        Assert.Equal(1, index.Value.Groups[0].Forums[0].TopicCount);
        Assert.Equal(2, index.Value.Groups[0].Forums[0].PostCount);
    }

    [Fact]
    public async Task UpdatePost_OtherUser_IsForbidden()
    {
        var author = new TestUser { UserId = "author", Name = "Author" };
        var service = ForumHarness.Create(database, author);
        int forumId = await CreateForumAsync();
        int topicId = (await service.CreateTopicAsync(new CreateTopicRequest(forumId, "Hello", "Mine"), null)).Value;
        var topic = await service.GetTopicAsync(topicId, 1, false);
        int postId = topic.Value.Posts[0].Id;

        var other = new TestUser { UserId = "other", Name = "Other" };
        var otherService = ForumHarness.Create(database, other);
        var updated = await otherService.UpdatePostAsync(new UpdatePostRequest(postId, "Changed"));

        Assert.Equal(Ardalis.Result.ResultStatus.Forbidden, updated.Status);
    }

    [Fact]
    public async Task UpdatePost_Administrator_Succeeds()
    {
        var author = new TestUser { UserId = "author", Name = "Author" };
        var service = ForumHarness.Create(database, author);
        int forumId = await CreateForumAsync();
        int topicId = (await service.CreateTopicAsync(new CreateTopicRequest(forumId, "Hello", "Mine"), null)).Value;
        int postId = (await service.GetTopicAsync(topicId, 1, false)).Value.Posts[0].Id;

        var admin = new TestUser { UserId = "admin", Name = "Admin", Administrator = true };
        var adminService = ForumHarness.Create(database, admin);
        var updated = await adminService.UpdatePostAsync(new UpdatePostRequest(postId, "Moderated"));

        Assert.True(updated.IsSuccess);
        Assert.Equal("Moderated", (await adminService.GetTopicAsync(topicId, 1, false)).Value.Posts[0].Text);
    }

    [Fact]
    public async Task Search_FindsPostText()
    {
        var user = new TestUser();
        var service = ForumHarness.Create(database, user);
        int forumId = await CreateForumAsync();
        await service.CreateTopicAsync(new CreateTopicRequest(forumId, "Welcome", "The secret word is lantern"), null);

        var found = await service.SearchAsync("lantern", null, 1);
        var missed = await service.SearchAsync("a", null, 1);

        Assert.True(found.IsSuccess);
        Assert.Single(found.Value.Topics);
        Assert.Equal(Ardalis.Result.ResultStatus.Invalid, missed.Status);
    }

    [Fact]
    public async Task Subscribe_Twice_DoesNotDuplicate()
    {
        var user = new TestUser();
        var service = ForumHarness.Create(database, user);
        int forumId = await CreateForumAsync();

        Assert.True((await service.SubscribeToForumAsync(forumId)).IsSuccess);
        Assert.True((await service.SubscribeToForumAsync(forumId)).IsSuccess);
        var list = await service.GetSubscriptionsAsync(1);

        Assert.Single(list.Value.Items);
    }

    [Fact]
    public async Task MoveTopic_UpdatesBothForums()
    {
        var admin = new TestUser { Administrator = true };
        var service = ForumHarness.Create(database, admin);
        int groupId = (await service.SaveGroupAsync(new SaveForumGroupRequest(null, "General", 1))).Value;
        int sourceId = (await service.SaveForumAsync(new SaveForumRequest(null, groupId, "Source", string.Empty, 1))).Value;
        int destinationId = (await service.SaveForumAsync(new SaveForumRequest(null, groupId, "Destination", string.Empty, 2))).Value;
        int topicId = (await service.CreateTopicAsync(new CreateTopicRequest(sourceId, "Move me", "Body"), null)).Value;

        var moved = await service.MoveTopicAsync(topicId, destinationId);
        var source = await service.GetForumAsync(sourceId, 1);
        var destination = await service.GetForumAsync(destinationId, 1);

        Assert.True(moved.IsSuccess);
        Assert.Empty(source.Value.Topics);
        Assert.Single(destination.Value.Topics);
        Assert.Equal(0, (await service.GetIndexAsync()).Value.Groups[0].Forums.Single(forum => forum.Id == sourceId).PostCount);
        Assert.Equal(1, (await service.GetIndexAsync()).Value.Groups[0].Forums.Single(forum => forum.Id == destinationId).PostCount);
    }

    [Fact]
    public async Task GetIndex_OtherTenant_DoesNotSeeGroups()
    {
        var admin = new TestUser { Administrator = true };
        var tenant = new TestTenant { TenantId = "1" };
        var service = ForumHarness.Create(database, admin, tenant);
        await service.SaveGroupAsync(new SaveForumGroupRequest(null, "Tenant one", 1));

        tenant.TenantId = "2";
        var other = await service.GetIndexAsync();

        Assert.Empty(other.Value.Groups);
    }

    [Fact]
    public async Task CreateTopic_Anonymous_IsForbidden()
    {
        var admin = new TestUser { Administrator = true };
        var service = ForumHarness.Create(database, admin);
        int forumId = await CreateForumAsync();

        var guest = new TestUser { UserId = null, Name = null };
        var guestService = ForumHarness.Create(database, guest);
        var created = await guestService.CreateTopicAsync(new CreateTopicRequest(forumId, "Nope", "Nope"), null);

        Assert.Equal(Ardalis.Result.ResultStatus.Forbidden, created.Status);
    }

    [Fact]
    public async Task DeleteGroup_WithForum_IsInvalid()
    {
        var admin = new TestUser { Administrator = true };
        var service = ForumHarness.Create(database, admin);
        int groupId = (await service.SaveGroupAsync(new SaveForumGroupRequest(null, "General", 1))).Value;
        await service.SaveForumAsync(new SaveForumRequest(null, groupId, "Chat", string.Empty, 1));

        var deleted = await service.DeleteGroupAsync(groupId);

        Assert.Equal(Ardalis.Result.ResultStatus.Invalid, deleted.Status);
    }

    [Fact]
    public async Task SendMessage_ToDisplayName_LandsInInbox()
    {
        var ada = new TestUser { UserId = "ada", Name = "Ada" };
        var grace = new TestUser { UserId = "grace", Name = "Grace" };
        var adaService = ForumHarness.Create(database, ada);
        var graceService = ForumHarness.Create(database, grace);
        await adaService.GetProfileAsync();
        await graceService.GetProfileAsync();

        var sent = await adaService.SendMessageAsync(new SendMessageRequest("Grace", "Hi", "Hello Grace"));
        var inbox = await graceService.GetInboxAsync(1);
        var opened = await graceService.GetMessageAsync(sent.Value);

        Assert.True(sent.IsSuccess);
        Assert.Single(inbox.Value.Items);
        Assert.Equal(1, inbox.Value.UnreadCount);
        Assert.True(opened.Value.IsRead);
        Assert.Equal(0, (await graceService.GetUnreadMessageCountAsync()).Value);
    }

    private async Task<int> CreateForumAsync()
    {
        var admin = new TestUser { Administrator = true, UserId = "admin", Name = "Admin" };
        var service = ForumHarness.Create(database, admin);
        int groupId = (await service.SaveGroupAsync(new SaveForumGroupRequest(null, "General", 1))).Value;
        return (await service.SaveForumAsync(new SaveForumRequest(null, groupId, "Chat", "Talk", 1))).Value;
    }
}
