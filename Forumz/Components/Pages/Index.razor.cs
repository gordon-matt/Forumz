using Heroicsoft.NetForge.Results;

namespace Forumz.Components.Pages;

/// <summary>The forum board, with structure editing for an administrator.</summary>
public partial class Index
{
    private ForumIndexModel? model;
    private ForumStructureModel? structure;
    private string? error;
    private bool loading = true;
    private bool relative = true;
    private bool feedsEnabled = true;
    private bool groupOpen;
    private bool forumOpen;
    private bool settingsOpen;
    private readonly GroupForm groupForm = new();
    private readonly ForumForm forumForm = new();
    private SettingsForm? settings;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        loading = model is null;
        var settingsResult = await Forums.GetSettingsAsync();
        relative = !settingsResult.IsSuccess || settingsResult.Value.RelativeDates;
        feedsEnabled = !settingsResult.IsSuccess || settingsResult.Value.FeedsEnabled;
        var result = await Forums.GetIndexAsync();
        if (!result.IsSuccess)
        {
            loading = false;
            error = result.GetDisplayMessage();
            model = null;
            structure = null;
            return;
        }

        error = null;
        model = result.Value;
        if (model.CanManage)
        {
            var editable = await Forums.GetStructureAsync();
            structure = editable.IsSuccess ? editable.Value : null;
            if (!editable.IsSuccess)
            {
                Toasts.ShowFailure(editable);
            }
        }
        else
        {
            structure = null;
        }

        loading = false;
    }

    private ForumGroupEditModel? GroupEdit(int id) =>
        structure?.Groups.FirstOrDefault(group => group.Id == id);

    private ForumEditModel? ForumEdit(int id) =>
        structure?.Groups.SelectMany(group => group.Forums).FirstOrDefault(forum => forum.Id == id);

    private void NewGroup()
    {
        groupForm.Id = null;
        groupForm.Name = string.Empty;
        groupForm.DisplayOrder = (structure?.Groups.Count ?? 0) + 1;
        groupOpen = true;
    }

    private void EditGroup(ForumGroupEditModel group)
    {
        groupForm.Id = group.Id;
        groupForm.Name = group.Name;
        groupForm.DisplayOrder = group.DisplayOrder;
        groupOpen = true;
    }

    private async Task SaveGroupAsync()
    {
        var result = await Forums.SaveGroupAsync(new SaveForumGroupRequest(groupForm.Id, groupForm.Name, groupForm.DisplayOrder));
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            return;
        }

        groupOpen = false;
        Toasts.Success("Group saved.");
        await LoadAsync();
    }

    private async Task DeleteGroupAsync(ForumGroupEditModel group)
    {
        bool confirmed = await Dialogs.ConfirmAsync("Delete this group?", group.Name, "Delete", isDestructive: true);
        if (!confirmed)
        {
            return;
        }

        var result = await Forums.DeleteGroupAsync(group.Id);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            return;
        }

        Toasts.Success("Group deleted.");
        await LoadAsync();
    }

    private void NewForum(ForumGroupEditModel group)
    {
        forumForm.Id = null;
        forumForm.GroupId = group.Id;
        forumForm.Name = string.Empty;
        forumForm.Description = string.Empty;
        forumForm.DisplayOrder = group.Forums.Count == 0 ? 1 : group.Forums.Max(forum => forum.DisplayOrder) + 1;
        forumOpen = true;
    }

    private void EditForum(ForumEditModel forum)
    {
        forumForm.Id = forum.Id;
        forumForm.GroupId = forum.GroupId;
        forumForm.Name = forum.Name;
        forumForm.Description = forum.Description;
        forumForm.DisplayOrder = forum.DisplayOrder;
        forumOpen = true;
    }

    private async Task SaveForumAsync()
    {
        var result = await Forums.SaveForumAsync(new SaveForumRequest(forumForm.Id, forumForm.GroupId, forumForm.Name, forumForm.Description, forumForm.DisplayOrder));
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            return;
        }

        forumOpen = false;
        Toasts.Success("Forum saved.");
        await LoadAsync();
    }

    private async Task DeleteForumAsync(ForumEditModel forum)
    {
        bool confirmed = await Dialogs.ConfirmAsync(
            "Delete this forum?",
            "Topics and posts in it are deleted too.",
            "Delete",
            isDestructive: true,
            forum.Name);
        if (!confirmed)
        {
            return;
        }

        var result = await Forums.DeleteForumAsync(forum.Id);
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            return;
        }

        Toasts.Success("Forum deleted.");
        await LoadAsync();
    }

    private async Task EditSettings()
    {
        var result = await Forums.GetSettingsAsync();
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            return;
        }

        var value = result.Value;
        settings = new SettingsForm
        {
            AllowUsersToEditPosts = value.AllowUsersToEditPosts,
            AllowUsersToDeletePosts = value.AllowUsersToDeletePosts,
            AllowUsersToManageSubscriptions = value.AllowUsersToManageSubscriptions,
            AllowPrivateMessages = value.AllowPrivateMessages,
            RelativeDates = value.RelativeDates,
            FeedsEnabled = value.FeedsEnabled,
            SignaturesEnabled = value.SignaturesEnabled,
            TopicsPageSize = value.TopicsPageSize,
            PostsPageSize = value.PostsPageSize,
            SearchPageSize = value.SearchPageSize,
            ActivePageSize = value.ActivePageSize,
            MessagesPageSize = value.MessagesPageSize,
            SubscriptionsPageSize = value.SubscriptionsPageSize,
            SubjectMaxLength = value.SubjectMaxLength,
            PostMaxLength = value.PostMaxLength,
            MessageSubjectMaxLength = value.MessageSubjectMaxLength,
            MessageTextMaxLength = value.MessageTextMaxLength,
            SearchMinimumLength = value.SearchMinimumLength,
            FeedCount = value.FeedCount,
        };
        settingsOpen = true;
    }

    private async Task SaveSettingsAsync()
    {
        if (settings is null)
        {
            return;
        }

        var result = await Forums.SaveSettingsAsync(new SaveForumSettingsRequest(
            settings.AllowUsersToEditPosts,
            settings.AllowUsersToDeletePosts,
            settings.AllowUsersToManageSubscriptions,
            settings.AllowPrivateMessages,
            settings.RelativeDates,
            settings.FeedsEnabled,
            settings.SignaturesEnabled,
            settings.TopicsPageSize,
            settings.PostsPageSize,
            settings.SearchPageSize,
            settings.ActivePageSize,
            settings.MessagesPageSize,
            settings.SubscriptionsPageSize,
            settings.SubjectMaxLength,
            settings.PostMaxLength,
            settings.MessageSubjectMaxLength,
            settings.MessageTextMaxLength,
            settings.SearchMinimumLength,
            settings.FeedCount));
        if (!result.IsSuccess)
        {
            Toasts.ShowFailure(result);
            return;
        }

        settingsOpen = false;
        feedsEnabled = settings.FeedsEnabled;
        relative = settings.RelativeDates;
        Toasts.Success("Settings saved.");
    }

    private sealed class GroupForm
    {
        public int? Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }
    }

    private sealed class ForumForm
    {
        public int? Id { get; set; }

        public int GroupId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }
    }

    private sealed class SettingsForm
    {
        public bool AllowUsersToEditPosts { get; set; }

        public bool AllowUsersToDeletePosts { get; set; }

        public bool AllowUsersToManageSubscriptions { get; set; }

        public bool AllowPrivateMessages { get; set; }

        public bool RelativeDates { get; set; }

        public bool FeedsEnabled { get; set; }

        public bool SignaturesEnabled { get; set; }

        public int TopicsPageSize { get; set; }

        public int PostsPageSize { get; set; }

        public int SearchPageSize { get; set; }

        public int ActivePageSize { get; set; }

        public int MessagesPageSize { get; set; }

        public int SubscriptionsPageSize { get; set; }

        public int SubjectMaxLength { get; set; }

        public int PostMaxLength { get; set; }

        public int MessageSubjectMaxLength { get; set; }

        public int MessageTextMaxLength { get; set; }

        public int SearchMinimumLength { get; set; }

        public int FeedCount { get; set; }
    }
}
