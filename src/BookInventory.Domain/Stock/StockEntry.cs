using BookInventory.BuildingBlocks.Domain;
using BookInventory.Domain.Core;

namespace BookInventory.Domain.Stock;

/// <summary>
/// Spec stock R4, R13: one movement of one item's stock. A child of <see cref="ItemStock"/>, created only by it, and never edited
/// or deleted afterwards (a mistake is corrected by a new entry). It carries its own company (platform P18) and who made it and
/// when (stamped by the persistence layer); it is the history itself, so the audit trail skips it (D3).
/// </summary>
[AuditIgnore]
public class StockEntry : TransactionDomainBase, ICompanyOwned
{
    private StockEntry()
    {
    }

    public Guid CompanyId { get; private set; }

    public Guid ItemStockId { get; private set; }

    public Guid ItemId { get; private set; }

    public StockType StockType { get; private set; }

    public MovementKind MovementKind { get; private set; }

    public int QuantityIn { get; private set; }

    public int QuantityOut { get; private set; }

    public DateOnly MovementDate { get; private set; }

    public string? DocumentType { get; private set; }

    public Guid? DocumentId { get; private set; }

    public string? DocumentNumber { get; private set; }

    public string? Note { get; private set; }

    public StockChange Change => new(MovementKind, QuantityIn, QuantityOut);

    Guid? IAuditCompanyScope.AuditCompanyId => CompanyId == Guid.Empty ? null : CompanyId;

    public void AssignCompany(Guid companyId) => CompanyId = CompanyAssignment.Apply(CompanyId, companyId);

    internal static StockEntry Of(ItemStock stock, StockMovement movement) =>
        new()
        {
            ItemStockId = stock.Id,
            ItemId = stock.ItemId,
            StockType = movement.StockType,
            MovementKind = movement.Change.Kind,
            QuantityIn = movement.Change.QuantityIn,
            QuantityOut = movement.Change.QuantityOut,
            MovementDate = movement.MovementDate,
            DocumentType = movement.Document?.DocumentType,
            DocumentId = movement.Document?.DocumentId,
            DocumentNumber = movement.Document?.DocumentNumber,
            Note = movement.Note
        };
}
