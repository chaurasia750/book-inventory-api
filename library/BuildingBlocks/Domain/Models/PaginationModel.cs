namespace BookInventory.BuildingBlocks.Domain.Models
{
    public abstract class PaginationModel
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public SortingOrder SortingOrder { get; set; } = SortingOrder.Asc;

        /// <summary>Entity property to sort by (validated against the entity before use).</summary>
        public string? ColName { get; set; }
    }

    public enum SortingOrder
    {
        Desc = 1,
        Asc = 0
    }
}
