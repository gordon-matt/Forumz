using Forumz.Core;
using Forumz.Services.Content;

namespace Forumz.Tests;

public class PostQuoteTests
{
    private readonly ForumContentRenderer renderer = new();

    [Fact]
    public void Format_Markdown_QuotesEachLineAndRendersAsABlockquote()
    {
        string quoted = PostQuote.Format("Blair Chen", "Hello\r\n\r\nthere", ContentFormat.Markdown, ContentFormat.Markdown, renderer);

        Assert.Equal("> **Blair Chen** wrote:\n>\n> Hello\n>\n> there\n\n", quoted);

        string html = renderer.ToHtml(quoted, ContentFormat.Markdown);
        Assert.Contains("<blockquote>", html, StringComparison.Ordinal);
        Assert.Contains("<strong>Blair Chen</strong>", html, StringComparison.Ordinal);
        Assert.Contains("Hello", html, StringComparison.Ordinal);
        Assert.Contains("there", html, StringComparison.Ordinal);
    }

    [Fact]
    public void Format_Markdown_EscapesTheAuthorName()
    {
        string quoted = PostQuote.Format("[click](javascript:alert(1))", "Hi", ContentFormat.Markdown, ContentFormat.Markdown, renderer);

        Assert.Contains("> **\\[click\\](javascript:alert(1))** wrote:", quoted, StringComparison.Ordinal);
        string html = renderer.ToHtml(quoted, ContentFormat.Markdown);
        Assert.Contains("[click](javascript:alert(1))", html, StringComparison.Ordinal);
        Assert.DoesNotContain("<a ", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<em>", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Format_Markdown_NestsAnExistingQuote()
    {
        string quoted = PostQuote.Format("Casey", "> Earlier\nStill", ContentFormat.Markdown, ContentFormat.Markdown, renderer);

        Assert.Contains("> > Earlier", quoted, StringComparison.Ordinal);
        Assert.Contains("> Still", quoted, StringComparison.Ordinal);
    }

    [Fact]
    public void Format_EmptyAuthorOrBody_UsesANameAndSkipsTheBody()
    {
        string quoted = PostQuote.Format("  ", "  ", ContentFormat.Markdown, ContentFormat.Markdown, renderer);

        Assert.Equal("> **Someone** wrote:\n\n", quoted);
    }

    [Fact]
    public void Format_Html_WrapsSanitizedBodyAndEncodesTheAuthor()
    {
        string quoted = PostQuote.Format(
            "<b>Ann</b>",
            "<p>Hello</p><script>alert(1)</script>",
            ContentFormat.Html,
            ContentFormat.Html,
            renderer);

        Assert.Contains("<blockquote><p><strong>&lt;b&gt;Ann&lt;/b&gt;</strong> wrote:</p>", quoted, StringComparison.Ordinal);
        Assert.Contains("<p>Hello</p>", quoted, StringComparison.Ordinal);
        Assert.DoesNotContain("<script", quoted, StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith("</blockquote><p><br></p>", quoted, StringComparison.Ordinal);
    }

    [Fact]
    public void Format_HtmlTarget_RendersMarkdownSource()
    {
        string quoted = PostQuote.Format("Blair", "A **bold** word.", ContentFormat.Markdown, ContentFormat.Html, renderer);

        Assert.Contains("<strong>Blair</strong>", quoted, StringComparison.Ordinal);
        Assert.Contains("<strong>bold</strong>", quoted, StringComparison.Ordinal);
        Assert.StartsWith("<blockquote>", quoted, StringComparison.Ordinal);
    }

    [Fact]
    public void Format_MarkdownTarget_TurnsHtmlSourceIntoWords()
    {
        string quoted = PostQuote.Format(
            "Ann",
            "<p>Hello <strong>there</strong></p><p>Next</p><script>alert(1)</script>",
            ContentFormat.Html,
            ContentFormat.Markdown,
            renderer);

        Assert.Equal("> **Ann** wrote:\n>\n> Hello there\n> Next\n\n", quoted);
        Assert.DoesNotContain("<", quoted, StringComparison.Ordinal);
    }
}
