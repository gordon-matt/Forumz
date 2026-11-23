using EasyChatz.Models;
using EasyChatz.Services;

namespace Forumz.Services;

/// <summary>Names for EasyChatz, taken from people who already have a forum profile.</summary>
public sealed class ForumChatUserDirectory(IRepository<ForumMember> members, IForumTenantScope tenants) : IChatUserDirectory
{
    /// <inheritdoc />
    public async Task<ChatUser?> FindAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        string tenant = Tenant;
        var member = await members.FindOneAsync(new SearchOptions<ForumMember>
        {
            Query = item => item.TenantId == tenant && item.UserId == userId,
            CancellationToken = cancellationToken,
        });

        return member is null ? null : ToChatUser(member);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, ChatUser>> FindManyAsync(
        IEnumerable<string> userIds,
        CancellationToken cancellationToken = default)
    {
        var ids = userIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.Ordinal).ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<string, ChatUser>(StringComparer.Ordinal);
        }

        string tenant = Tenant;
        var found = await members.FindAsync(new SearchOptions<ForumMember>
        {
            Query = member => member.TenantId == tenant && ids.Contains(member.UserId),
            CancellationToken = cancellationToken,
        });

        return found.ToDictionary(member => member.UserId, ToChatUser, StringComparer.Ordinal);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ChatUser>> SearchAsync(string term, int take, CancellationToken cancellationToken = default)
    {
        int count = Math.Clamp(take, 1, 20);
        string tenant = Tenant;
        string query = term?.Trim().ToLowerInvariant() ?? string.Empty;
        var found = await members.FindAsync(new SearchOptions<ForumMember>
        {
            Query = member => member.TenantId == tenant
                && member.DisplayName != string.Empty
                && (query.Length == 0 || member.DisplayName.ToLower().Contains(query)),
            OrderBy = rows => rows.OrderBy(member => member.DisplayName),
            PageSize = count,
            CancellationToken = cancellationToken,
        });

        return found.Select(ToChatUser).ToList();
    }

    private string Tenant => string.IsNullOrWhiteSpace(tenants.TenantId) ? string.Empty : tenants.TenantId.Trim();

    private static ChatUser ToChatUser(ForumMember member) =>
        new(member.UserId, member.DisplayName, false, false);
}