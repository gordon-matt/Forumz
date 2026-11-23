namespace Forumz.Services;

/// <summary>The folder a file manager would use, so an image uploaded without one still appears there later.</summary>
public interface IPostImageLocation
{
    /// <summary>Absolute path of the upload folder.</summary>
    string PhysicalPath { get; }

    /// <summary>Public URL prefix, with a leading slash. A trailing slash is added when it is missing.</summary>
    string RequestPath { get; }
}

/// <summary>Saves an image from the author's computer and returns the address to put in a post.</summary>
public interface IPostImageStore
{
    /// <summary>
    /// Stores a JPEG, PNG, GIF, or WebP image of at most 2 MB.
    /// The returned address is root-relative, such as <c>/media/ab12.png</c>.
    /// </summary>
    /// <param name="content">Image bytes.</param>
    /// <param name="contentType">The image media type, such as <c>image/png</c>.</param>
    /// <param name="cancellationToken">Stops the write when the request goes away.</param>
    Task<Result<string>> SaveAsync(Stream content, string contentType, CancellationToken cancellationToken = default);
}

/// <summary>
/// Writes post images into <see cref="IPostImageLocation"/>, which is the Pickstack upload folder when the host has one.
/// Each file gets its own name, so one upload cannot replace another.
/// </summary>
public sealed class PostImageStore(IPostImageLocation location, ILogger<PostImageStore> logger) : IPostImageStore
{
    private const long MaxBytes = 2 * 1024 * 1024;

    /// <inheritdoc />
    public async Task<Result<string>> SaveAsync(Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        string? extension = ExtensionFor(contentType);
        if (extension is null)
        {
            return Result<string>.Invalid(new ValidationError(nameof(contentType), "Use a JPEG, PNG, GIF, or WebP image."));
        }

        if (content is null)
        {
            return Result<string>.Invalid(new ValidationError(nameof(content), "Choose an image."));
        }

        if (string.IsNullOrWhiteSpace(location.PhysicalPath))
        {
            return Result<string>.Error("The site has no upload folder.");
        }

        Directory.CreateDirectory(location.PhysicalPath);
        string name = Guid.NewGuid().ToString("N") + extension;
        string destination = Path.Combine(location.PhysicalPath, name);
        bool keep = false;

        try
        {
            await using (var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                byte[] buffer = new byte[81920];
                long total = 0;
                int read;
                while ((read = await content.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    total += read;
                    if (total > MaxBytes)
                    {
                        return Result<string>.Invalid(new ValidationError(nameof(content), "Keep the image under 2 MB."));
                    }

                    await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                }

                if (total == 0)
                {
                    return Result<string>.Invalid(new ValidationError(nameof(content), "Choose an image."));
                }
            }

            keep = true;
        }
        finally
        {
            if (!keep && File.Exists(destination))
            {
                File.Delete(destination);
            }
        }

        string prefix = location.RequestPath.Replace('\\', '/').Trim();
        if (!prefix.StartsWith('/'))
        {
            prefix = "/" + prefix;
        }

        if (!prefix.EndsWith('/'))
        {
            prefix += "/";
        }

        string url = prefix + name;
        logger.LogInformation("Stored a forum post image at {Url}", url);
        return Result<string>.Success(url);
    }

    private static string? ExtensionFor(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return null;
        }

        string type = contentType.Split(';', 2)[0].Trim();
        return type.ToLowerInvariant() switch
        {
            "image/jpeg" or "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            "image/webp" => ".webp",
            _ => null,
        };
    }
}