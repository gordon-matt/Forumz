namespace Forumz.Models;

/// <summary>
/// Profile details a host can store for a user. Only the photo is used today; further fields can be added here.
/// </summary>
/// <param name="UserId">Host user id.</param>
/// <param name="PhotoUri">A URI the browser can load. Empty when the user has no photo.</param>
public sealed record ProfileDataModel(string UserId, string PhotoUri);
