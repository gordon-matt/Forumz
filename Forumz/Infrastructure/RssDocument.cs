using System.Globalization;
using System.Net;
using System.Text;

namespace Forumz.Infrastructure;

/// <summary>A small RSS 2.0 writer. Paths on the feed are under the forum's base URL.</summary>
internal static class RssDocument
{
    public static string Write(FeedModel feed, string baseUrl)
    {
        var builder = new StringBuilder();
        builder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        builder.Append("<rss version=\"2.0\"><channel>");
        Element(builder, "title", feed.Title);
        Element(builder, "description", feed.Description);
        Element(builder, "link", baseUrl);
        foreach (var item in feed.Items)
        {
            builder.Append("<item>");
            Element(builder, "title", item.Title);
            Element(builder, "link", Combine(baseUrl, item.Path));
            Element(builder, "description", item.Summary);
            Element(builder, "pubDate", item.PublishedOnUtc.ToString("R", CultureInfo.InvariantCulture));
            builder.Append("</item>");
        }

        builder.Append("</channel></rss>");
        return builder.ToString();
    }

    private static void Element(StringBuilder builder, string name, string value)
    {
        builder.Append('<').Append(name).Append('>');
        builder.Append(WebUtility.HtmlEncode(value));
        builder.Append("</").Append(name).Append('>');
    }

    private static string Combine(string baseUrl, string path) =>
        baseUrl.TrimEnd('/') + "/" + path.TrimStart('/');
}