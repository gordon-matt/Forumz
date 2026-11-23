using Forumz.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Forumz.Tests;

public sealed class PostImageStoreTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "forumz-images-" + Guid.NewGuid().ToString("N"));

    public PostImageStoreTests()
    {
        Directory.CreateDirectory(root);
    }

    public void Dispose() => Directory.Delete(root, recursive: true);

    [Fact]
    public async Task Save_WritesANewFileInTheFileManagerFolder()
    {
        var images = Create();

        var saved = await images.SaveAsync(new MemoryStream([0x89, 0x50, 0x4E, 0x47]), "image/png");

        Assert.True(saved.IsSuccess);
        Assert.StartsWith("/Media/Uploads/", saved.Value, StringComparison.Ordinal);
        Assert.EndsWith(".png", saved.Value, StringComparison.Ordinal);
        Assert.Single(Directory.GetFiles(root));
    }

    [Fact]
    public async Task Save_NotAnImage_IsRejected()
    {
        var images = Create();

        var saved = await images.SaveAsync(new MemoryStream([1, 2, 3, 4]), "text/plain");

        Assert.False(saved.IsSuccess);
        Assert.Empty(Directory.GetFiles(root));
    }

    [Fact]
    public async Task Save_LargerThanTwoMegabytes_IsRejected()
    {
        var images = Create();

        var saved = await images.SaveAsync(new MemoryStream(new byte[(2 * 1024 * 1024) + 1]), "image/png");

        Assert.False(saved.IsSuccess);
        Assert.Empty(Directory.GetFiles(root));
    }

    private PostImageStore Create() =>
        new(new FixedLocation(root, "/Media/Uploads/"), NullLogger<PostImageStore>.Instance);

    private sealed class FixedLocation(string physicalPath, string requestPath) : IPostImageLocation
    {
        public string PhysicalPath => physicalPath;

        public string RequestPath => requestPath;
    }
}