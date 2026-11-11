using Forumz.Core;
using Forumz.Services.Content;

namespace Forumz.Tests;

public class ForumContentRendererTests
{
    private readonly ForumContentRenderer renderer = new();

    [Fact]
    public void ToHtml_Markdown_RendersEmphasisAndDropsScripts()
    {
        string html = renderer.ToHtml("A **bold** word.\n\n<script>alert(1)</script>", ContentFormat.Markdown);

        Assert.Contains("<strong>bold</strong>", html, StringComparison.Ordinal);
        Assert.DoesNotContain("<script", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ToHtml_Html_StripsScripts()
    {
        string html = renderer.ToHtml("<p>Hello</p><script>alert(1)</script>", ContentFormat.Html);

        Assert.Contains("<p>Hello</p>", html, StringComparison.Ordinal);
        Assert.DoesNotContain("<script", html, StringComparison.OrdinalIgnoreCase);
    }
}