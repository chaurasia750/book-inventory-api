using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace BookInventory.Api.Hosting;

/// <summary>
/// P13/UAC3: the API serves the three Angular apps from its own <c>wwwroot</c> so the browser only ever talks to one
/// origin (the refresh cookie is <c>SameSite=Strict</c>): the public site at <c>/</c>, the admin app at <c>/admin</c> and
/// the member app at <c>/member</c>. Nothing is served when there is no <c>wwwroot</c> (development, where the UI runs on
/// its own dev server and proxies <c>/api</c> here), so the API behaves the same without the UI.
/// <list type="bullet">
/// <item>A deep link that is not a file answers the <c>index.html</c> of the app it belongs to, so a reload works.</item>
/// <item>An unknown <c>/api</c> path stays a 404; it never turns into an HTML page.</item>
/// <item>A path with a file extension that does not exist is a 404, not <c>index.html</c> (the fallback ignores paths that look like files).</item>
/// <item>Cache: <c>index.html</c> and every unhashed file revalidate (<c>no-cache</c>), files whose name carries the build
/// hash are immutable for a year.</item>
/// </list>
/// </summary>
public static partial class SpaHosting
{
    private static readonly string[] AppFolders = ["admin", "member"];

    /// <summary>Angular names hashed files <c>main-A1B2C3D4.js</c>; such a name never changes its content.</summary>
    [GeneratedRegex(@"-[A-Z0-9]{8}\.[a-z0-9]+$")]
    private static partial Regex HashedFileName();

    public static bool HasWebRoot(this WebApplication app) =>
        app.Environment.WebRootPath is { Length: > 0 } root && File.Exists(Path.Combine(root, "index.html"));

    public static WebApplication UseSpaStaticFiles(this WebApplication app)
    {
        if (app.HasWebRoot())
            app.UseStaticFiles(new StaticFileOptions { OnPrepareResponse = context => SetCacheHeader(context.File.Name, context.Context.Response) });

        return app;
    }

    public static WebApplication MapSpaFallback(this WebApplication app)
    {
        if (!app.HasWebRoot())
            return app;

        app.MapFallback(context => AnswerDeepLinkAsync(app, context));
        return app;
    }

    private static async Task AnswerDeepLinkAsync(WebApplication app, HttpContext context)
    {
        var index = IndexFileFor(context.Request.Path);
        var isPageRequest = HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method);
        var file = index is null ? null : app.Environment.WebRootFileProvider.GetFileInfo(index);

        if (!isPageRequest || file is not { Exists: true })
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        context.Response.ContentType = "text/html; charset=utf-8";
        context.Response.Headers.CacheControl = "no-cache";
        await context.Response.SendFileAsync(file);
    }

    /// <summary>The <c>index.html</c> a deep link belongs to, or <c>null</c> when the path is not for a page (an API path).</summary>
    private static string? IndexFileFor(PathString path)
    {
        if (path.StartsWithSegments("/api"))
            return null;

        foreach (var folder in AppFolders)
        {
            if (path.StartsWithSegments("/" + folder))
                return $"{folder}/index.html";
        }

        return "index.html";
    }

    private static void SetCacheHeader(string fileName, HttpResponse response) =>
        response.Headers[HeaderNames.CacheControl] = HashedFileName().IsMatch(fileName)
            ? "public,max-age=31536000,immutable"
            : "no-cache";
}
