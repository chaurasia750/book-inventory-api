using System.ComponentModel.DataAnnotations;

namespace BookInventory.Infrastructure.Documents.Configuration
{
    /// <summary>
    /// The headless Chromium used to render PDFs (platform P25, PQ2, PQ3), from <c>Documents:Pdf</c>.
    /// <see cref="BrowserPath"/> is optional: when it is empty the Chromium-based browser already installed on the
    /// machine is found (Chrome, else Edge). Nothing is ever downloaded.
    /// </summary>
    public sealed class DocumentsPdfOptions : IValidatableObject
    {
        public const string SectionName = "Documents:Pdf";

        /// <summary>Full path of the browser executable; empty means "find the installed Chrome, else Edge".</summary>
        public string BrowserPath { get; set; } = string.Empty;

        /// <summary>How long one render (launch, load, print) may take.</summary>
        public int TimeoutSeconds { get; set; } = 60;

        /// <summary>How many browsers may run at once; each one holds roughly 100-200 MB, so more requests wait their turn.</summary>
        public int MaxConcurrentRenders { get; set; } = 2;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MaxConcurrentRenders is < 1 or > 16)
                yield return new ValidationResult("Documents:Pdf:MaxConcurrentRenders must be between 1 and 16.", [nameof(MaxConcurrentRenders)]);

            if (TimeoutSeconds is < 1 or > 600)
                yield return new ValidationResult("Documents:Pdf:TimeoutSeconds must be between 1 and 600.", [nameof(TimeoutSeconds)]);
        }
    }
}
