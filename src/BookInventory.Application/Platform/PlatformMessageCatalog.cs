using BookInventory.BuildingBlocks.Domain.Documents;

namespace BookInventory.Application.Platform
{
    /// <summary>The human text of every platform (BuildingBlocks) message code; the code travels, the text is looked up (R55).</summary>
    public static class PlatformMessageCatalog
    {
        public static IReadOnlyDictionary<string, string> Messages { get; } =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [DocumentMessages.RenderFailed] = "The document could not be rendered. Please try again; if it keeps failing, contact support."
            };
    }
}
