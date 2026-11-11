using Microsoft.AspNetCore.Components.Web;

namespace Forumz.Infrastructure;

/// <summary>Interactive Server without prerender, so a topic view is not counted twice.</summary>
internal static class ForumzRenderMode
{
    public static IComponentRenderMode Interactive { get; } = new InteractiveServerRenderMode(prerender: false);
}
