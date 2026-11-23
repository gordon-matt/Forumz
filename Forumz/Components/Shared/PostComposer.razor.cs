using Forumz.Services.Content;
using Heroicsoft.NetForge.Components.Dialogs;
using Heroicsoft.NetForge.Components.Editors;
using Microsoft.JSInterop;

namespace Forumz.Components.Shared;

/// <summary>Markdown or the host's Pickstack rich text editor, for a new post or a reply.</summary>
public partial class PostComposer : IDisposable
{
    // Root-absolute. The forum document's base href is the mount path, and a
    // "./_content/..." import would be requested under that path and miss the script.
    private const string ModulePath = "/_content/Forumz/forumz.js";

    private int generation;
    private ElementReference root;
    private ElementReference imageFile;
    private MarkdownEditor? markdown;
    private string? pickError;
    private DotNetObjectReference<PostComposer>? selfReference;
    private Task<IJSObjectReference>? module;

    /// <summary>Accessible name for the Markdown box.</summary>
    [Parameter]
    public string Label { get; set; } = "Post";

    /// <summary>The post source.</summary>
    [Parameter]
    public string Text { get; set; } = string.Empty;

    /// <summary>Raised when the source changes.</summary>
    [Parameter]
    public EventCallback<string> TextChanged { get; set; }

    /// <summary>Markdown or rich text.</summary>
    [Parameter]
    public ContentFormat Format { get; set; } = ContentFormat.Markdown;

    /// <summary>Raised when the author switches editor.</summary>
    [Parameter]
    public EventCallback<ContentFormat> FormatChanged { get; set; }

    [Inject]
    private ForumEditorCapabilities Capabilities { get; set; } = default!;

    [Inject]
    private IForumContentRenderer Renderer { get; set; } = default!;

    [Inject]
    private DialogService Dialogs { get; set; } = default!;

    [Inject]
    private IJSRuntime Js { get; set; } = default!;

    [Inject]
    private ForumUrls Urls { get; set; } = default!;

    /// <summary>Inserts a quote at the caret and scrolls the editor into view.</summary>
    /// <param name="snippet">Quote source in the editor's current format.</param>
    public async Task InsertQuoteAsync(string snippet)
    {
        if (string.IsNullOrWhiteSpace(snippet))
        {
            return;
        }

        string? written = null;
        try
        {
            written = await (await ModuleAsync()).InvokeAsync<string?>("insertQuote", root, snippet);
        }
        catch (JSDisconnectedException)
        {
            return;
        }
        catch (JSException)
        {
            written = null;
        }

        // Null means the Markdown preview is showing, or the rich text editor has not mounted.
        // The bound text is what that preview and the later editor both read.
        if (written is null)
        {
            await TextChanged.InvokeAsync(Append(Text, snippet));
            return;
        }

        if (!string.Equals(written, Text, StringComparison.Ordinal))
        {
            await TextChanged.InvokeAsync(written);
        }
    }

    private static string Append(string? current, string snippet) =>
        string.IsNullOrWhiteSpace(current) ? snippet : current.TrimEnd() + "\n\n" + snippet;

    private async Task SwitchAsync(ContentFormat target)
    {
        if (target == Format)
        {
            return;
        }

        bool hasContent = !string.IsNullOrWhiteSpace(Text);
        if (hasContent && !await Dialogs.ConfirmAsync("Change editor", "The post is converted so the wording stays. Switching back keeps it as it is now.", "Change editor"))
        {
            return;
        }

        string converted = hasContent && target == ContentFormat.Html
            ? Renderer.ToHtml(Text, ContentFormat.Markdown)
            : Text;

        generation++;
        await TextChanged.InvokeAsync(converted);
        await FormatChanged.InvokeAsync(target);
    }

    private async Task ChooseImageAsync()
    {
        pickError = null;
        if (Capabilities.HasFileManager)
        {
            await PickFromManagerAsync();
            return;
        }

        try
        {
            await (await ModuleAsync()).InvokeVoidAsync("chooseFile", imageFile);
        }
        catch (JSDisconnectedException)
        {
        }
        catch (JSException)
        {
            pickError = "The image could not be chosen.";
        }
    }

    private async Task PickFromManagerAsync()
    {
        selfReference ??= DotNetObjectReference.Create(this);

        try
        {
            await (await ModuleAsync()).InvokeVoidAsync("pickFile", selfReference, nameof(InsertImageAsync), "image", string.Empty);
        }
        catch (JSException)
        {
            pickError = "The file picker did not open. Check that the site registers a Pickstack file manager.";
        }
    }

    private async Task UploadLocalImageAsync()
    {
        pickError = null;

        try
        {
            await using var upload = await Js.InvokeAsync<IJSObjectReference>("import", Urls.Asset("forumz-upload.js"));
            var outcome = await upload.InvokeAsync<ImageUploadOutcome>("upload", imageFile, Urls.To("post-image"), "image");
            if (!outcome.Ok || string.IsNullOrWhiteSpace(outcome.Url))
            {
                pickError = string.IsNullOrWhiteSpace(outcome.Message)
                    ? "The image could not be uploaded. Try a smaller image."
                    : outcome.Message;
                return;
            }

            await InsertImageAsync(outcome.Url);
        }
        catch (JSDisconnectedException)
        {
        }
        catch (JSException)
        {
            pickError = "The image could not be uploaded. Try a smaller image.";
        }
    }

    /// <summary>Called by the file picker. A blank address is ignored. Any other address is inserted as a Markdown image.</summary>
    /// <param name="url">The address the picker returned.</param>
    /// <returns>A task that completes once the image has been inserted, or immediately when there is nothing to insert.</returns>
    [JSInvokable]
    public async Task InsertImageAsync(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || markdown is null)
        {
            return;
        }

        await markdown.InsertTextAsync($"![Describe the image]({url.Replace(" ", "%20", StringComparison.Ordinal)})");
    }

    /// <inheritdoc />
    public void Dispose() => selfReference?.Dispose();

    private Task<IJSObjectReference> ModuleAsync() =>
        module ??= Js.InvokeAsync<IJSObjectReference>("import", ModulePath).AsTask();

    private sealed class ImageUploadOutcome
    {
        public bool Ok { get; set; }

        public string Message { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;
    }
}