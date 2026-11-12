using Forumz.Core;
using Forumz.Services;
using Heroicsoft.NetForge.Abstractions.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Forumz.Tests;

public sealed class DefaultProfileProviderTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "forumz-photos-" + Guid.NewGuid().ToString("N"));

    public DefaultProfileProviderTests() => Directory.CreateDirectory(root);

    public void Dispose() => Directory.Delete(root, recursive: true);

    [Fact]
    public async Task Get_NoFile_ReturnsEmptyPhoto()
    {
        var profiles = Create();

        var profile = await profiles.GetAsync("user-1");

        Assert.Equal("user-1", profile.UserId);
        Assert.Equal(string.Empty, profile.PhotoUri);
    }

    [Fact]
    public async Task Save_WritesUnderTheConfiguredFolder()
    {
        var profiles = Create();
        byte[] bytes = [0x89, 0x50, 0x4E, 0x47];

        var saved = await profiles.SaveAsync("user-1", new MemoryStream(bytes), "image/png");

        Assert.True(saved.IsSuccess);
        Assert.Equal("user-1", saved.Value.UserId);
        Assert.StartsWith("/forumz/avatars/", saved.Value.PhotoUri, StringComparison.Ordinal);
        Assert.EndsWith(".png", saved.Value.PhotoUri.Split('?')[0], StringComparison.Ordinal);
        Assert.Single(Directory.GetFiles(Path.Combine(root, "forumz", "avatars")));

        var removed = await profiles.RemoveAsync("user-1");
        Assert.True(removed.IsSuccess);
        Assert.Equal(string.Empty, (await profiles.GetAsync("user-1")).PhotoUri);
        Assert.Empty(Directory.GetFiles(Path.Combine(root, "forumz", "avatars")));
    }

    [Fact]
    public async Task Save_AnotherUser_IsForbidden()
    {
        var profiles = Create();

        var saved = await profiles.SaveAsync("someone-else", new MemoryStream([1, 2, 3, 4]), "image/png");

        Assert.False(saved.IsSuccess);
    }

    [Fact]
    public async Task Save_PathOutsideWwwroot_IsRejected()
    {
        string escape = "forumz-escape-" + Guid.NewGuid().ToString("N");
        var profiles = Create(path: "../" + escape);

        var saved = await profiles.SaveAsync("user-1", new MemoryStream([1, 2, 3, 4]), "image/png");

        Assert.False(saved.IsSuccess);
        Assert.False(Directory.Exists(Path.Combine(Directory.GetParent(root)!.FullName, escape)));
    }

    private DefaultProfileProvider Create(string path = "forumz/avatars") =>
        new(
            Options.Create(new ForumzOptions { ProfilePhotoPath = path }),
            new TestWebRoot(root),
            new TestUser { UserId = "user-1" },
            new TestTenant(),
            NullLogger<DefaultProfileProvider>.Instance);

    private sealed class TestWebRoot(string path) : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Test";

        public string ApplicationName { get; set; } = "Forumz.Tests";

        public string ContentRootPath { get; set; } = path;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();

        public string WebRootPath { get; set; } = path;

        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
