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

    private async Task PickImageAsync()
    {
        pickError = null;
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

    /// <summary>Called by the file picker with the chosen image address.</summary>
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
}