namespace BookInventory.BuildingBlocks.Application.Documents
{
    /// <summary>
    /// How a document is laid out on the page (platform P25). The defaults are A4 portrait with small margins, which is
    /// what a business document needs; a module changes only what its own document needs.
    /// </summary>
    public sealed record HtmlToPdfOptions
    {
        /// <summary>Paper width in inches (A4 = 8.27).</summary>
        public decimal PaperWidthInches { get; init; } = 8.27m;

        /// <summary>Paper height in inches (A4 = 11.69).</summary>
        public decimal PaperHeightInches { get; init; } = 11.69m;

        public bool Landscape { get; init; }

        public decimal MarginTopInches { get; init; } = 0.35m;

        public decimal MarginBottomInches { get; init; } = 0.35m;

        public decimal MarginLeftInches { get; init; } = 0.30m;

        public decimal MarginRightInches { get; init; } = 0.30m;

        /// <summary>HTML shown in the page margin at the top of every page; null prints none. The header/footer area is the page margin, so a header needs a top margin to be visible.</summary>
        public string? HeaderHtml { get; init; }

        /// <summary>HTML shown in the page margin at the bottom of every page; null prints none. <c>span.pageNumber</c> and <c>span.totalPages</c> are filled by the browser.</summary>
        public string? FooterHtml { get; init; }

        public decimal Scale { get; init; } = 1.0m;

        public bool PrintBackground { get; init; } = true;

        public bool DisplayHeaderFooter => !string.IsNullOrEmpty(HeaderHtml) || !string.IsNullOrEmpty(FooterHtml);
    }

    /// <summary>
    /// Turns an HTML string into a PDF (platform P25). The one shared capability every module asks; the implementation
    /// (a headless Chromium) is chosen by configuration and is unknown to the callers. A failure raises
    /// <c>document.render_failed</c> - never a half-written document.
    /// </summary>
    public interface IHtmlToPdfRenderer
    {
        Task<byte[]> RenderAsync(string html, HtmlToPdfOptions? pageOptions = null, CancellationToken cancellationToken = default);
    }
}
