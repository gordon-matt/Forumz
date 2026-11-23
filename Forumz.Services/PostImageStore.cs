using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Hosting;

namespace Forumz.Services;

/// <summary>Saves an image from the author's computer and returns the address to put in a post.</summary>
public interface IPostImageStore
{
    /// <summary>
    /// Stores a JPEG, PNG, GIF, or WebP image of at most 2 MB.
    /// The returned address is root-relative, such as <c>/forumz/images/ab12.png</c>.
    /// </summary>
    /// <param name="content">Image bytes.</param>
    /// <param name="contentType">The image media type, such as <c>image/png</c>.</param>
    /// <param name="cancellationToken">Stops the write when the request goes away.</param>
    Task<Result<string>> SaveAsync(Stream content, string contentType, CancellationToken cancellationToken = default);
}

/// <summary>
/// Writes post images under wwwroot, in <see cref="ForumzOptions.PostImagePath"/>.
/// A folder that leaves wwwroot is rejected. Each file gets its own name, so one upload cannot replace another.
/// </summary>
public sealed class PostImageStore(
    IOptions<ForumzOptions> options,
    IWebHostEnvironment environment,
    IForumTenantScope tenants,
    ILogger<PostImageStore> logger) : IPostImageStore
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

        var directory = ImageDirectory();
        if (!directory.IsSuccess)
        {
            return directory.ValidationErrors.Any()
                ? Result<string>.Invalid(directory.ValidationErrors)
                : Result<string>.Error(directory.Errors.FirstOrDefault() ?? "The image could not be saved.");
        }

        Directory.CreateDirectory(directory.Value.Full);
        string name = Guid.NewGuid().ToString("N") + extension;
        string destination = Path.Combine(directory.Value.Full, name);
        bool keep = false;

        try
        {
            await using (var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                var buffer = new byte[81920];
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

        string url = "/" + directory.Value.Relative + "/" + name;
        logger.LogInformation("Stored a forum post image at {Url}", url);
        return Result<string>.Success(url);
    }

    private Result<(string Full, string Relative)> ImageDirectory()
    {
        if (string.IsNullOrEmpty(environment.WebRootPath))
        {
            return Result<(string, string)>.Error("The site has no wwwroot directory.");
        }

        string? relative = DefaultProfileProvider.NormalizePath(options.Value.PostImagePath);
        if (relative is null)
        {
            return Result<(string, string)>.Invalid(new ValidationError(nameof(ForumzOptions.PostImagePath), "The post image folder must stay under wwwroot."));
        }

        string? tenant = SafeTenant(tenants.TenantId);
        if (tenant is not null)
        {
            relative += "/" + tenant;
        }

        string root = Path.GetFullPath(environment.WebRootPath);
        string full = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
        string prefix = root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return full.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? Result<(string, string)>.Success((full, relative))
            : Result<(string, string)>.Invalid(new ValidationError(nameof(ForumzOptions.PostImagePath), "The post image folder must stay under wwwroot."));
    }

    private static string? SafeTenant(string? tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return null;
        }

        string trimmed = tenantId.Trim();
        return trimmed.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_')
            ? trimmed
            : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(trimmed))).ToLowerInvariant()[..16];
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
