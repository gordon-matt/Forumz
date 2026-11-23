using Pickstack.FileManager;

namespace Forumz;

/// <summary>
/// The upload folder Pickstack's file manager already uses. An image saved here is in that browser
/// when the forum starts using the file manager.
/// </summary>
internal sealed class PickstackPostImageLocation(IFileStorage storage) : IPostImageLocation
{
    /// <inheritdoc />
    public string PhysicalPath => storage.PhysicalPath;

    /// <inheritdoc />
    public string RequestPath => storage.RequestPath;
}