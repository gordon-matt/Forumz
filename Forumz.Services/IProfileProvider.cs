namespace Forumz.Services;

/// <summary>
/// Reads and stores a user's profile. Forumz registers <see cref="DefaultProfileProvider"/>,
/// which keeps the photo under wwwroot. Register another implementation to store it somewhere else.
/// A registration added after <c>AddForumz</c> is the one that is used.
/// </summary>
public interface IProfileProvider
{
    /// <summary>The profile for this user. <see cref="ProfileDataModel.PhotoUri"/> is empty when there is no photo.</summary>
    /// <param name="userId">Host user id.</param>
    /// <param name="cancellationToken">Stops the lookup when the page goes away.</param>
    Task<ProfileDataModel> GetAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a new photo for this user and returns the profile with the URI the browser should load.
    /// JPEG, PNG, GIF, and WebP only, and at most 2 MB.
    /// </summary>
    /// <param name="userId">Host user id. The forum passes the signed-in member.</param>
    /// <param name="content">Image bytes.</param>
    /// <param name="contentType">The image media type, such as <c>image/png</c>.</param>
    /// <param name="cancellationToken">Stops the write when the page goes away.</param>
    Task<Result<ProfileDataModel>> SaveAsync(string userId, Stream content, string contentType, CancellationToken cancellationToken = default);

    /// <summary>Removes this user's photo.</summary>
    /// <param name="userId">Host user id. The forum passes the signed-in member.</param>
    /// <param name="cancellationToken">Stops the delete when the page goes away.</param>
    Task<Result> RemoveAsync(string userId, CancellationToken cancellationToken = default);
}