using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;

namespace Forumz;

/// <summary>Mounts the Blazor forum and its RSS feeds.</summary>
public static class EndpointRouteBuilderExtensions
{
    extension(IEndpointRouteBuilder endpoints)
    {
        /// <summary>
        /// Mounts the forum at <see cref="ForumzOptions.BasePath"/>. Reading is anonymous by default.
        /// Require authorization on the returned group only when the whole forum is private:
        /// <c>app.MapForumz().RequireAuthorization()</c>.
        /// </summary>
        /// <returns>The route group, including the circuit and the feeds.</returns>
        public RouteGroupBuilder MapForumz()
        {
            var options = endpoints.ServiceProvider.GetRequiredService<IOptions<ForumzOptions>>().Value;
            var group = endpoints.MapGroup(options.BasePath);

            group.MapGet("_assets/{name}", EmbeddedAssets.Serve).ExcludeFromDescription();
            group.MapGet("_framework/blazor.web.js", ServeBlazorScript).ExcludeFromDescription();
            group.MapGet("feed", (IForumService forums, HttpContext context, CancellationToken cancellationToken) =>
                FeedAsync(forums, context, null, cancellationToken));
            group.MapGet("forum/{forumId:int}/feed", (int forumId, IForumService forums, HttpContext context, CancellationToken cancellationToken) =>
                FeedAsync(forums, context, forumId, cancellationToken));

            group.MapRazorComponents<Components.ForumzApp>()
                .AddInteractiveServerRenderMode()
                .DisableAntiforgery();

            return group;
        }
    }

    private static async Task<IResult> FeedAsync(IForumService forums, HttpContext context, int? forumId, CancellationToken cancellationToken)
    {
        var result = await forums.GetFeedAsync(forumId, cancellationToken);
        if (!result.IsSuccess)
        {
            int status = result.Status == Ardalis.Result.ResultStatus.Forbidden
                ? StatusCodes.Status403Forbidden
                : StatusCodes.Status404NotFound;
            return Results.StatusCode(status);
        }

        var request = context.Request;
        var options = context.RequestServices.GetRequiredService<IOptions<ForumzOptions>>().Value;
        string baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}{options.BasePath}";
        return Results.Text(RssDocument.Write(result.Value, baseUrl), "application/rss+xml");
    }

    private static IResult ServeBlazorScript(IWebHostEnvironment environment)
    {
        var file = environment.WebRootFileProvider.GetFileInfo("_framework/blazor.web.js");
        return !file.Exists
            ? Results.Problem(
                "blazor.web.js was not found in the web root. Forumz needs the host to be built with the ASP.NET Core Web SDK.",
                statusCode: StatusCodes.Status404NotFound)
            : file.PhysicalPath is { } physicalPath
                ? Results.File(physicalPath, "text/javascript")
                : Results.Stream(file.CreateReadStream(), "text/javascript", lastModified: file.LastModified);
    }
}