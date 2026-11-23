using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Forumz.Services.Content;

/// <summary>Turns a post into the quote an author drops into a reply.</summary>
public static partial class PostQuote
{
    /// <summary>
    /// A quote in <paramref name="targetFormat"/>. Markdown stays editable source. HTML is the
    /// sanitized body wrapped in a blockquote. Quoting HTML into Markdown keeps the words only.
    /// </summary>
    /// <param name="authorName">Name shown on the quoted post. Empty becomes "Someone".</param>
    /// <param name="source">The quoted post's stored source.</param>
    /// <param name="sourceFormat">How <paramref name="source"/> is stored.</param>
    /// <param name="targetFormat">How the reply is being written.</param>
    /// <param name="renderer">Renders and sanitizes when the quote has to become HTML, or HTML has to become words.</param>
    /// <returns>The quote, ready to insert into a reply written in <paramref name="targetFormat"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is null.</exception>
    public static string Format(
        string? authorName,
        string? source,
        ContentFormat sourceFormat,
        ContentFormat targetFormat,
        IForumContentRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);

        string author = DisplayName(authorName);
        if (targetFormat == ContentFormat.Html)
        {
            return ToHtml(author, renderer.ToHtml(source, sourceFormat));
        }

        string markdown = sourceFormat == ContentFormat.Markdown
            ? source ?? string.Empty
            : HtmlToText(renderer.ToHtml(source, ContentFormat.Html));
        return ToMarkdown(author, markdown);
    }

    private static string ToMarkdown(string author, string? body)
    {
        var builder = new StringBuilder();
        builder.Append("> **").Append(EscapeMarkdown(author)).Append("** wrote:");

        string text = Normalize(body);
        if (text.Length > 0)
        {
            builder.Append("\n>");
            foreach (string line in text.Split('\n'))
            {
                builder.Append('\n').Append('>');
                if (line.Length > 0)
                {
                    builder.Append(' ').Append(line);
                }
            }
        }

        // A blank line ends the quote, so the next sentence the author types is their own.
        builder.Append("\n\n");
        return builder.ToString();
    }

    private static string ToHtml(string author, string? bodyHtml)
    {
        string body = string.IsNullOrWhiteSpace(bodyHtml) ? string.Empty : bodyHtml.Trim();
        return $"<blockquote><p><strong>{WebUtility.HtmlEncode(author)}</strong> wrote:</p>{body}</blockquote><p><br></p>";
    }

    private static string DisplayName(string? author)
    {
        if (string.IsNullOrWhiteSpace(author))
        {
            return "Someone";
        }

        string collapsed = author.Replace("\r\n", " ", StringComparison.Ordinal).Replace('\r', ' ').Replace('\n', ' ').Trim();
        return collapsed.Length == 0 ? "Someone" : collapsed;
    }

    private static string EscapeMarkdown(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (char character in value)
        {
            if (character is '\\' or '*' or '_' or '`' or '[' or ']' or '<')
            {
                builder.Append('\\');
            }

            builder.Append(character);
        }

        return builder.ToString();
    }

    private static string Normalize(string? source) => string.IsNullOrWhiteSpace(source)
            ? string.Empty
            : source.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Trim();

    private static string HtmlToText(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        string text = Breaks().Replace(html, "\n");
        text = Blocks().Replace(text, "\n");
        text = Tags().Replace(text, string.Empty);
        text = WebUtility.HtmlDecode(text).Replace('\u00A0', ' ');
        text = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        text = BlankLines().Replace(text, "\n\n");
        return text.Trim();
    }

    [GeneratedRegex(@"<br\s*/?>", RegexOptions.IgnoreCase)]
    private static partial Regex Breaks();

    [GeneratedRegex(@"</(?:p|div|h[1-6]|li|tr|blockquote|pre)>", RegexOptions.IgnoreCase)]
    private static partial Regex Blocks();

    [GeneratedRegex("<[^>]+>", RegexOptions.None)]
    private static partial Regex Tags();

    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex BlankLines();
}