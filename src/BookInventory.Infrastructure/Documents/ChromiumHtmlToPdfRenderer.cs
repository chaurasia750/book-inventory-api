using System.Globalization;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Application.Documents;
using BookInventory.BuildingBlocks.Domain.Documents;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Infrastructure.Documents.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace BookInventory.Infrastructure.Documents;

/// <summary>
/// Renders HTML to a PDF with a headless Chromium (platform P25). The browser is the one already installed on the
/// machine (Chrome, else Edge) or the path in <c>Documents:Pdf:BrowserPath</c>; nothing is downloaded (PQ2). It talks
/// to the browser through PuppeteerSharp (PQ3). A missing browser, a launch failure or a render failure all raise
/// <c>document.render_failed</c> - never a corrupt or empty document. One browser is launched per render in Phase 1.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton, typeof(IHtmlToPdfRenderer))]
public sealed class ChromiumHtmlToPdfRenderer(
    IOptions<DocumentsPdfOptions> options,
    ILogger<ChromiumHtmlToPdfRenderer> logger) : IHtmlToPdfRenderer
{
    private readonly SemaphoreSlim _browserSlots = new(options.Value.MaxConcurrentRenders);

    public async Task<byte[]> RenderAsync(string html, HtmlToPdfOptions? pageOptions = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(html);

        var layout = pageOptions ?? new HtmlToPdfOptions();
        var settings = options.Value;
        var executablePath = ResolveExecutablePath(settings.BrowserPath);

        await _browserSlots.WaitAsync(cancellationToken);
        try
        {
            var pdf = await RenderCoreAsync(html, layout, executablePath, settings.TimeoutSeconds);
            logger.LogDebug("Rendered a document of {Bytes} bytes with {Browser}.", pdf.Length, executablePath);
            return pdf;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Document render failed with {Browser}.", executablePath);
            throw new ServerFailureException(DocumentMessages.RenderFailed, ex.Message, ex);
        }
        finally
        {
            _browserSlots.Release();
        }
    }

    /// <summary>Launches the browser, loads the HTML and prints it; the caller turns any failure into `document.render_failed`.</summary>
    private static async Task<byte[]> RenderCoreAsync(string html, HtmlToPdfOptions layout, string executablePath, int timeoutSeconds)
    {
        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            ExecutablePath = executablePath,
            Timeout = timeoutSeconds * 1000,
            Args = ["--no-sandbox", "--disable-dev-shm-usage", "--disable-gpu"]
        });

        var chromePage = await browser.NewPageAsync();
        await chromePage.SetContentAsync(html);
        return await chromePage.PdfDataAsync(BuildPdfOptions(layout, timeoutSeconds));
    }

    private static PdfOptions BuildPdfOptions(HtmlToPdfOptions layout, int timeoutSeconds) => new()
    {
        Width = Inches(layout.PaperWidthInches),
        Height = Inches(layout.PaperHeightInches),
        Landscape = layout.Landscape,
        PrintBackground = layout.PrintBackground,
        Scale = layout.Scale,
        DisplayHeaderFooter = layout.DisplayHeaderFooter,
        HeaderTemplate = layout.HeaderHtml ?? "<span></span>",
        FooterTemplate = layout.FooterHtml ?? "<span></span>",
        MarginOptions = new MarginOptions
        {
            Top = Inches(layout.MarginTopInches),
            Bottom = Inches(layout.MarginBottomInches),
            Left = Inches(layout.MarginLeftInches),
            Right = Inches(layout.MarginRightInches)
        },
        Timeout = timeoutSeconds * 1000
    };

    private static string Inches(decimal value) => $"{value.ToString(CultureInfo.InvariantCulture)}in";

    /// <summary>The configured browser, else the Chromium-based browser already installed on the machine (Chrome, else Edge).</summary>
    private static string ResolveExecutablePath(string configured)
    {
        if (!string.IsNullOrWhiteSpace(configured))
            return configured;

        foreach (var candidate in BrowserCandidates())
        {
            if (!string.IsNullOrWhiteSpace(candidate) && File.Exists(candidate))
                return candidate;
        }

        throw new ServerFailureException(DocumentMessages.RenderFailed,
            "No Chromium-based browser (Chrome or Edge) was found; set Documents:Pdf:BrowserPath.");
    }

    private static IEnumerable<string> BrowserCandidates()
    {
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        yield return Path.Combine(programFiles, "Google", "Chrome", "Application", "chrome.exe");
        yield return Path.Combine(programFilesX86, "Google", "Chrome", "Application", "chrome.exe");
        yield return Path.Combine(localAppData, "Google", "Chrome", "Application", "chrome.exe");
        yield return Path.Combine(programFiles, "Microsoft", "Edge", "Application", "msedge.exe");
        yield return Path.Combine(programFilesX86, "Microsoft", "Edge", "Application", "msedge.exe");
        yield return "/usr/bin/google-chrome";
        yield return "/usr/bin/chromium";
        yield return "/usr/bin/chromium-browser";
        yield return "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome";
        yield return "/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge";
    }
}
