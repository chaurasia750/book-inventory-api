namespace BookInventory.BuildingBlocks.Domain
{
    /// <summary>
    /// Resolves the human-readable text for a message code. Every module registers its own codes and texts once
    /// (see <c>AddMessageCatalog</c>); nothing user-facing is hard-coded elsewhere.
    /// </summary>
    public interface IMessageCatalog
    {
        /// <summary>The text for <paramref name="code"/>, or null when the code is unknown.</summary>
        string? Find(string code);
    }
}
