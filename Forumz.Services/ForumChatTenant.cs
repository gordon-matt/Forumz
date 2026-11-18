using EasyChatz.Services;

namespace Forumz.Services;

/// <summary>
/// Passes the forum's current tenant through to EasyChatz.
/// A host that replaces <see cref="IForumTenantScope"/>, including Inferno's Flatmate scope, covers direct messages as well.
/// </summary>
public sealed class ForumChatTenant(IForumTenantScope tenants) : IChatTenant
{
    /// <inheritdoc />
    public string? TenantId => tenants.TenantId;
}
