namespace Forumz.Components;

/// <summary>
/// Addresses of forum pages. Relative while the forum is its own document, because that document's
/// base href is already the mount path. Rooted at the mount path when the pages are drawn in the host's document.
/// </summary>
public sealed class ForumUrls(IOptions<ForumzOptions> options)
{
    /// <summary>The forum is drawn inside the host layout rather than its own document.</summary>
    public bool InHost => options.Value.PublicLayout is not null;

    /// <summary>The forum home.</summary>
    public string Home => To(string.Empty);

    /// <summary>Stylesheet address. Rooted when the host document's base href is the site root.</summary>
    public string Stylesheet => Asset("forumz.css");

    /// <summary>
    /// An embedded asset. Rooted when the host document's base href is the site root, so a module
    /// import does not resolve under that base and miss the mount path.
    /// </summary>
    /// <param name="name">File name under the forum's assets, such as <c>forumz.css</c>.</param>
    public string Asset(string name)
    {
        string relative = EmbeddedAssets.Url(name);
        return InHost ? $"{options.Value.BasePath.TrimEnd('/')}/{relative}" : relative;
    }

    /// <summary>A page or feed under the forum. <paramref name="relative"/> may include a query string.</summary>
    /// <param name="relative">Path relative to the forum, such as <c>topic/1</c>.</param>
    public string To(string relative)
    {
        string path = Normalize(relative);
        if (!InHost)
        {
            return path.Length == 0 ? "./" : path;
        }

        string root = options.Value.BasePath.TrimEnd('/');
        return path.Length == 0 ? root : $"{root}/{path}";
    }

    private static string Normalize(string relative)
    {
        string value = relative.Trim();
        if (value is "" or "." or "./")
        {
            return string.Empty;
        }

        return value.StartsWith("./", StringComparison.Ordinal) ? value[2..] : value.TrimStart('/');
    }
}
