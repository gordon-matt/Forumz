using Microsoft.AspNetCore.Components.Endpoints;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;

namespace Forumz;

/// <summary>
/// Picks the forum document or the host's router for this request. Feeds and the stylesheet stay either way.
/// </summary>
internal sealed class ForumPresentationPolicy : MatcherPolicy, IEndpointSelectorPolicy
{
    /// <inheritdoc />
    public override int Order => 1000;

    /// <inheritdoc />
    public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints) =>
        endpoints.Any(endpoint => IsForumDocument(endpoint));

    /// <inheritdoc />
    public Task ApplyAsync(HttpContext httpContext, CandidateSet candidates)
    {
        var options = httpContext.RequestServices.GetRequiredService<IOptions<ForumzOptions>>().Value;
        if (!httpContext.Request.Path.StartsWithSegments(options.BasePath, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        bool ownTopBar = httpContext.RequestServices.GetRequiredService<IForumPresentation>().UseOwnTopBar;
        bool forumIsCandidate = false;
        for (int index = 0; index < candidates.Count; index++)
        {
            if (candidates.IsValidCandidate(index) && IsForumDocument(candidates[index].Endpoint))
            {
                forumIsCandidate = true;
                break;
            }
        }

        // Leave the host in place when the forum document is not a candidate for this path.
        if (ownTopBar && !forumIsCandidate)
        {
            return Task.CompletedTask;
        }

        for (int index = 0; index < candidates.Count; index++)
        {
            if (!candidates.IsValidCandidate(index))
            {
                continue;
            }

            var root = candidates[index].Endpoint.Metadata.GetMetadata<RootComponentMetadata>();
            if (root is null)
            {
                continue;
            }

            bool forumDocument = root.Type == typeof(Components.ForumzApp);
            if (ownTopBar != forumDocument)
            {
                candidates.SetValidity(index, false);
            }
        }

        return Task.CompletedTask;
    }

    private static bool IsForumDocument(Endpoint endpoint) =>
        endpoint.Metadata.GetMetadata<RootComponentMetadata>()?.Type == typeof(Components.ForumzApp);
}
