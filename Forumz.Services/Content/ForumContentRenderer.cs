using Ganss.Xss;
using Markdig;

namespace Forumz.Services.Content;

/// <summary>Turns a post's source into the HTML a reader is shown. The source itself stays in the database.</summary>
public interface IForumContentRenderer
{
    /// <summary>
    /// Sanitized HTML. Markdown is rendered first, and raw HTML inside Markdown is dropped.
    /// Empty or white-space source returns an empty string.
    /// </summary>
    /// <param name="source">The stored Markdown or HTML.</param>
    /// <param name="format">Which of the two <paramref name="source"/> is.</param>
    /// <returns>HTML a page can render, or an empty string.</returns>
    string ToHtml(string? source, ContentFormat format);
}

/// <summary>Markdig plus a sanitizer, so neither editor can put a script into a post.</summary>
public sealed class ForumContentRenderer : IForumContentRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .DisableHtml()
        .Build();

    private readonly HtmlSanitizer sanitizer = new();

    /// <inheritdoc />
    public string ToHtml(string? source, ContentFormat format)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return string.Empty;
        }

        // Markdown posts do not accept raw HTML. Rich text is the author's HTML, then cleaned.
        string html = format == ContentFormat.Html ? source : Markdown.ToHtml(source, Pipeline);
        return sanitizer.Sanitize(html);
    }
}