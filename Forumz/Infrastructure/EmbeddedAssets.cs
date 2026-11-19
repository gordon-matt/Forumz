using Heroicsoft.NetForge.WebAssets;

namespace Forumz.Infrastructure;

/// <summary>Forum stylesheet and scripts, served from the assembly at <c>/_content/Forumz/</c>.</summary>
internal static class EmbeddedAssets
{
    /// <summary>Root path of an embedded file, including its content hash.</summary>
    public static string Url(string name) => EmbeddedContent.Url(typeof(EmbeddedAssets).Assembly, name);
}
