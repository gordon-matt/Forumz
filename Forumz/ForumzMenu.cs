using Heroicsoft.NetForge.Components.Navigation;

namespace Forumz;

/// <summary>Puts the forum on a host that draws the shared NetForge menu. The link leaves this document.</summary>
public sealed class ForumzMenuContributor(IOptions<ForumzOptions> options) : INetForgeMenuContributor
{
    /// <inheritdoc />
    public string Name => "Forumz";

    /// <inheritdoc />
    public string DefaultMenu => "Forumz";

    /// <inheritdoc />
    public void Contribute(INetForgeMenuGroupBuilder group, Action<NetForgeMenuItemOptions>? configure)
    {
        group.AddCustom(item =>
        {
            item.Title = options.Value.Title;
            item.Icon = "chat";
            item.Url = options.Value.BasePath;
            item.External = true;
            configure?.Invoke(item);
        });
    }
}

/// <summary>Places the forum on a shared menu.</summary>
public static class ForumzMenuExtensions
{
    extension(INetForgeMenuGroupBuilder menu)
    {
        /// <summary>Adds the forum. The link is a full page load because the forum is its own Blazor document.</summary>
        /// <param name="configure">Replaces the title, icon, or address.</param>
        /// <returns>The same group.</returns>
        public INetForgeMenuGroupBuilder AddForumz(Action<NetForgeMenuItemOptions>? configure = null) =>
            menu.AddContributor("Forumz", configure);
    }
}
