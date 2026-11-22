using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Hosting;

namespace Forumz.Services;

/// <summary>
/// Stores profile photos as files under wwwroot, in <see cref="ForumzOptions.ProfilePhotoPath"/>.
/// Saving or removing a photo for anyone but the signed-in member is forbidden. A folder that leaves wwwroot is rejected.
/// A missing photo, or a site with no wwwroot, is reported by <see cref="IProfileProvider.GetAsync"/> as an empty photo address.
/// </summary>
public sealed class DefaultProfileProvider(
    IOptions<ForumzOptions> options,
    IWebHostEnvironment environment,
    IUserContextService users,
    IForumTenantScope tenants,
    ILogger<DefaultProfileProvider> logger) : IProfileProvider
{
    private const long MaxBytes = 2 * 1024 * 1024;

    private static readonly string[] Extensions = [".jpg", ".png", ".gif", ".webp"];

    /// <inheritdoc />
    public Task<ProfileDataModel> GetAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.FromResult(new ProfileDataModel(string.Empty, string.Empty));
        }

        var directory = PhotoDirectory();
        if (!directory.IsSuccess)
        {
            return Task.FromResult(new ProfileDataModel(userId, string.Empty));
        }

        string? relative = NormalizePath(options.Value.ProfilePhotoPath);
        string key = FileKey(userId);
        foreach (string extension in Extensions)
        {
            string path = Path.Combine(directory.Value, key + extension);
            if (!File.Exists(path))
            {
                continue;
            }

            long version = File.GetLastWriteTimeUtc(path).Ticks;
            string uri = "/" + relative + "/" + key + extension + "?v=" + version.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return Task.FromResult(new ProfileDataModel(userId, uri));
        }

        return Task.FromResult(new ProfileDataModel(userId, string.Empty));
    }

    /// <inheritdoc />
    public async Task<Result<ProfileDataModel>> SaveAsync(string userId, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var allowed = RequireSelf(userId);
        if (!allowed.IsSuccess)
        {
            return allowed.To<ProfileDataModel>();
        }

        string? extension = ExtensionFor(contentType);
        if (extension is null)
        {
            return Result<ProfileDataModel>.Invalid(new ValidationError(nameof(contentType), "Use a JPEG, PNG, GIF, or WebP image."));
        }

        if (content is null)
        {
            return Result<ProfileDataModel>.Invalid(new ValidationError(nameof(content), "Choose an image."));
        }

        var directory = PhotoDirectory();
        if (!directory.IsSuccess)
        {
            return directory.To<ProfileDataModel>();
        }

        Directory.CreateDirectory(directory.Value);
        string key = FileKey(userId);
        string temporary = Path.Combine(directory.Value, key + ".upload");
        string destination = Path.Combine(directory.Value, key + extension);

        try
        {
            await using (var output = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var buffer = new byte[81920];
                long total = 0;
                int read;
                while ((read = await content.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    total += read;
                    if (total > MaxBytes)
                    {
                        return Result<ProfileDataModel>.Invalid(new ValidationError(nameof(content), "Keep the photo under 2 MB."));
                    }

                    await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                }

                if (total == 0)
                {
                    return Result<ProfileDataModel>.Invalid(new ValidationError(nameof(content), "Choose an image."));
                }
            }

            foreach (string existing in Extensions)
            {
                string path = Path.Combine(directory.Value, key + existing);
                if (!string.Equals(path, destination, StringComparison.OrdinalIgnoreCase) && File.Exists(path))
                {
                    File.Delete(path);
                }
            }

            File.Move(temporary, destination, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary))
            {
                File.Delete(temporary);
            }
        }

        logger.LogInformation("User {UserId} updated their forum profile photo", userId);
        return Result<ProfileDataModel>.Success(await GetAsync(userId, cancellationToken));
    }

    /// <inheritdoc />
    public Task<Result> RemoveAsync(string userId, CancellationToken cancellationToken = default)
    {
        var allowed = RequireSelf(userId);
        if (!allowed.IsSuccess)
        {
            return Task.FromResult(allowed);
        }

        var directory = PhotoDirectory();
        if (!directory.IsSuccess)
        {
            return Task.FromResult(directory.ToVoid());
        }

        string key = FileKey(userId);
        foreach (string extension in Extensions)
        {
            string path = Path.Combine(directory.Value, key + extension);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        logger.LogInformation("User {UserId} removed their forum profile photo", userId);
        return Task.FromResult(Result.Success());
    }

    private Result RequireSelf(string userId)
    {
        string? current = users.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(current))
        {
            return Result.Forbidden("Sign in to change your photo.");
        }

        return string.Equals(current, userId, StringComparison.Ordinal)
            ? Result.Success()
            : Result.Forbidden("You can only change your own photo.");
    }

    private Result<string> PhotoDirectory()
    {
        if (string.IsNullOrEmpty(environment.WebRootPath))
        {
            return Result<string>.Error("The site has no wwwroot directory.");
        }

        string? relative = NormalizePath(options.Value.ProfilePhotoPath);
        if (relative is null)
        {
            return Result<string>.Invalid(new ValidationError(nameof(ForumzOptions.ProfilePhotoPath), "The profile photo folder must stay under wwwroot."));
        }

        string root = Path.GetFullPath(environment.WebRootPath);
        string full = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
        string prefix = root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return full.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? Result<string>.Success(full)
            : Result<string>.Invalid(new ValidationError(nameof(ForumzOptions.ProfilePhotoPath), "The profile photo folder must stay under wwwroot."));
    }

    private string FileKey(string userId)
    {
        string tenant = string.IsNullOrWhiteSpace(tenants.TenantId) ? string.Empty : tenants.TenantId.Trim();
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(tenant + "\n" + userId));
        return Convert.ToHexString(hash).ToLowerInvariant();
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

    /// <summary>A folder under wwwroot, or null when the value could leave that folder.</summary>
    internal static string? NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        string trimmed = path.Trim().Replace('\\', '/').Trim('/');
        if (trimmed.Length == 0 || trimmed.Contains(':'))
        {
            return null;
        }

        string[] parts = trimmed.Split('/');
        return parts.Any(part => part.Length == 0 || part is "." or "..") ? null : trimmed;
    }
}
