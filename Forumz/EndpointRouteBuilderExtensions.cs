using Heroicsoft.NetForge.Abstractions.Auth;
using Heroicsoft.NetForge.Results;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;

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
        /// The forum document is always mapped. <see cref="IForumPresentation"/> leaves a request on the
        /// host's router when the host layout should draw it, and serves the document when the forum keeps its own top bar.
        /// </summary>
        /// <returns>The route group, including the circuit and the feeds.</returns>
        public RouteGroupBuilder MapForumz()
        {
            var options = endpoints.ServiceProvider.GetRequiredService<IOptions<ForumzOptions>>().Value;
            var group = endpoints.MapGroup(options.BasePath);

            group.MapGet("_assets/{name}", EmbeddedAssets.Serve).ExcludeFromDescription();
            // The browser file stream's chunks never arrive on a host circuit, and the framework
            // then treats the wait as a fatal circuit error. A form post does not use that stream.
            group.MapPost("profile-photo", UploadPhotoAsync).DisableAntiforgery().ExcludeFromDescription();
            group.MapGet("feed", (IForumService forums, HttpContext context, CancellationToken cancellationToken) =>
                FeedAsync(forums, context, null, cancellationToken));
            group.MapGet("forum/{forumId:int}/feed", (int forumId, IForumService forums, HttpContext context, CancellationToken cancellationToken) =>
                FeedAsync(forums, context, forumId, cancellationToken));

            group.MapGet("_framework/blazor.web.js", ServeBlazorScript).ExcludeFromDescription();
            // The document base href is the mount path. Blazor resolves a "./_content/..."
            // import against that base, so the same static assets must also answer there.
            group.MapGet("_content/{**path}", ServeContent).ExcludeFromDescription();

            group.MapRazorComponents<Components.ForumzApp>()
                .AddInteractiveServerRenderMode()
                .DisableAntiforgery();

            return group;
        }
    }

    private static async Task<IResult> UploadPhotoAsync(
        HttpRequest request,
        IUserContextService users,
        IProfileProvider profiles,
        CancellationToken cancellationToken)
    {
        if (!users.IsAuthenticated())
        {
            return Results.Unauthorized();
        }

        string? userId = users.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Results.Unauthorized();
        }

        if (!request.HasFormContentType)
        {
            return Results.BadRequest(new { detail = "Choose an image." });
        }

        var form = await request.ReadFormAsync(cancellationToken);
        var file = form.Files.GetFile("photo");
        if (file is null || file.Length == 0)
        {
            return Results.BadRequest(new { detail = "Choose an image." });
        }

        await using var content = file.OpenReadStream();
        var saved = await profiles.SaveAsync(userId, content, file.ContentType, cancellationToken);
        if (saved.IsSuccess)
        {
            return Results.Ok(new { photoUri = saved.Value.PhotoUri });
        }

        string detail = saved.GetDisplayMessage();
        return saved.Status == Ardalis.Result.ResultStatus.Forbidden
            ? Results.Json(new { detail }, statusCode: StatusCodes.Status403Forbidden)
            : Results.BadRequest(new { detail });
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
        // text/xml opens in the browser. application/rss+xml is treated as a download.
        return Results.Text(RssDocument.Write(result.Value, baseUrl), "text/xml; charset=utf-8");
    }

    private static IResult ServeContent(string path, IWebHostEnvironment environment)
    {
        if (path.Contains("..", StringComparison.Ordinal))
        {
            return Results.NotFound();
        }

        var file = environment.WebRootFileProvider.GetFileInfo($"_content/{path}");
        if (!file.Exists)
        {
            return Results.NotFound();
        }

        var types = new FileExtensionContentTypeProvider();
        if (!types.TryGetContentType(path, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        return file.PhysicalPath is { } physicalPath
            ? Results.File(physicalPath, contentType)
            : Results.Stream(file.CreateReadStream(), contentType, lastModified: file.LastModified);
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