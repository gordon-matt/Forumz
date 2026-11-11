using Pickstack.FileManager;
using Pickstack.RichTextEditor;

namespace Forumz.Infrastructure;

/// <summary>
/// Which editors this site offers. The host registers a rich text editor and a file manager with
/// Pickstack; Forumz only asks whether they are there.
/// </summary>
internal sealed class ForumEditorCapabilities(IServiceProvider services, IOptions<ForumzOptions> options)
{
    public bool HasRichTextEditor { get; } = services.GetService<IRichTextEditor>() is not null;

    public bool HasFileManager { get; } = services.GetService<IFileManager>() is not null;

    public bool AllowsMarkdown => options.Value.EditorMode != ForumEditorMode.RichText || !HasRichTextEditor;

    public bool AllowsRichText => options.Value.EditorMode != ForumEditorMode.Markdown && HasRichTextEditor;

    public bool CanChooseFormat => AllowsMarkdown && AllowsRichText;

    public ContentFormat DefaultFormat => (AllowsMarkdown, AllowsRichText) switch
    {
        (true, true) => options.Value.DefaultContentFormat,
        (false, true) => ContentFormat.Html,
        _ => ContentFormat.Markdown,
    };
}