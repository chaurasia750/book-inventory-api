namespace BookInventory.BuildingBlocks.Domain.Documents
{
    /// <summary>The message codes of the shared document capability (platform P25). The code travels; the text is looked up in the catalogue.</summary>
    public static class DocumentMessages
    {
        /// <summary>The document could not be rendered (HTTP 500).</summary>
        public const string RenderFailed = "document.render_failed";
    }
}
