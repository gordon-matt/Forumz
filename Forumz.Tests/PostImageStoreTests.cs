using Forumz.Core;
using Forumz.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Forumz.Tests;

public sealed class PostImageStoreTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "forumz-images-" + Guid.NewGuid().ToString("N"));

    public PostImageStoreTests() => Directory.CreateDirectory(root);

    public void Dispose() => Directory.Delete(root, recursive: true);

    [Fact]
    public async Task Save_WritesANewFileUnderTheConfiguredFolder()
    {
        var images = Create();

        var saved = await images.SaveAsync(new MemoryStream([0x89, 0x50, 0x4E, 0x47]), "image/png");

        Assert.True(saved.IsSuccess);
        Assert.StartsWith("/forumz/images/", saved.Value, StringComparison.Ordinal);
        Assert.EndsWith(".png", saved.Value, StringComparison.Ordinal);
        Assert.Single(Directory.GetFiles(Path.Combine(root, "forumz", "images")));
    }

    [Fact]
    public async Task Save_Tenant_UsesItsOwnFolder()
    {
        var images = Create(tenant: "42");

        var saved = await images.SaveAsync(new MemoryStream([1, 2, 3, 4]), "image/jpeg");

        Assert.True(saved.IsSuccess);
        Assert.StartsWith("/forumz/images/42/", saved.Value, StringComparison.Ordinal);
        Assert.EndsWith(".jpg", saved.Value, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Save_NotAnImage_IsRejected()
    {
        var images = Create();

        var saved = await images.SaveAsync(new MemoryStream([1, 2, 3, 4]), "text/plain");

        Assert.False(saved.IsSuccess);
        Assert.False(Directory.Exists(Path.Combine(root, "forumz", "images")));
    }

    [Fact]
    public async Task Save_LargerThanTwoMegabytes_IsRejected()
    {
        var images = Create();

        var saved = await images.SaveAsync(new MemoryStream(new byte[(2 * 1024 * 1024) + 1]), "image/png");

        Assert.False(saved.IsSuccess);
        Assert.Empty(Directory.GetFiles(Path.Combine(root, "forumz", "images")));
    }

    [Fact]
    public async Task Save_PathOutsideWwwroot_IsRejected()
    {
        string escape = "forumz-image-escape-" + Guid.NewGuid().ToString("N");
        var images = Create(path: "../" + escape);

        var saved = await images.SaveAsync(new MemoryStream([1, 2, 3, 4]), "image/png");

        Assert.False(saved.IsSuccess);
        Assert.False(Directory.Exists(Path.Combine(Directory.GetParent(root)!.FullName, escape)));
    }

    private PostImageStore Create(string path = "forumz/images", string? tenant = null) =>
        new(
            Options.Create(new ForumzOptions { PostImagePath = path }),
            new ImageWebRoot(root),
            new TestTenant { TenantId = tenant },
            NullLogger<PostImageStore>.Instance);

    private sealed class ImageWebRoot(string path) : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Test";

        public string ApplicationName { get; set; } = "Forumz.Tests";

        public string ContentRootPath { get; set; } = path;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();

        public string WebRootPath { get; set; } = path;

        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
