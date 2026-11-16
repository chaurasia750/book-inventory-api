using BookInventory.Application.Item;
using BookInventory.Application.Price;
using BookInventory.Application.Sales;
using BookInventory.Application.Vendor;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Purchase;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Purchase;

/// <summary>
/// R2, R4, R10: turns a vendor return request into the domain's input, asking each module through its capability: the vendor
/// (vendor R15, active only, Q5), the items (item R16), the purchase price that starts a rate (price R19, Q3) and the company's printed
/// identity (company R17-R20, through the one snapshot builder of the sales module). Every value rule is the aggregate's.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class VendorReturnComposer(PriceCompanyGuard companyGuard, IVendorDirectory vendors, IItemDirectory items, IPurchasePriceDirectory prices, InvoiceParties parties)
{
    /// <summary>P20: a write needs a company in the session.</summary>
    public void RequireCompany() => companyGuard.Require();

    public async Task<VendorReturnInput> ComposeAsync(MakeVendorReturnRequest request, VendorReturnStamp stamp, CancellationToken cancellationToken)
    {
        var vendor = await vendors.GetAsync(request.VendorId!.Value, cancellationToken) ?? throw new ValidationFailureException(PurchaseMessages.VendorNotFound);
        if (!vendor.IsActive)
            throw new ValidationFailureException(PurchaseMessages.VendorInactive);

        return new VendorReturnInput
        {
            Number = stamp.Number,
            ReturnDate = stamp.ReturnDate,
            Today = stamp.Today,
            Vendor = new ReturnVendor(vendor.Id, vendor.Code, vendor.Name),
            CompanySnapshot = await parties.CompanySnapshotAsync(cancellationToken),
            Remark = request.Remark,
            Lines = await LinesAsync(request.Lines ?? [], stamp.ReturnDate, cancellationToken)
        };
    }

    /// <summary>R4: each item must exist and be active; a line without a rate takes the item's purchase price on the return date.</summary>
    private async Task<IReadOnlyList<VendorReturnLineInput>> LinesAsync(IReadOnlyList<VendorReturnLineRequest> lines, DateOnly date, CancellationToken cancellationToken)
    {
        if (lines.Count > VendorReturn.MaxLines)
            throw new ValidationFailureException(PurchaseMessages.TooManyLines);

        var ids = lines.Select(l => l.ItemId!.Value).Distinct().ToList();
        var references = await items.ReferencesAsync(ids, cancellationToken);
        var purchasePrices = await prices.PurchasePricesAsync([.. lines.Where(l => l.Rate is null).Select(l => l.ItemId!.Value).Distinct()], date, cancellationToken);
        return [.. lines.Select(l => LineOf(l, references, purchasePrices))];
    }

    private static VendorReturnLineInput LineOf(
        VendorReturnLineRequest line, IReadOnlyDictionary<Guid, ItemReference> references, IReadOnlyDictionary<Guid, PurchasePriceReference> purchasePrices)
    {
        var item = references.GetValueOrDefault(line.ItemId!.Value) ?? throw new ValidationFailureException(PurchaseMessages.ItemNotFound);
        if (!item.IsActive)
            throw new ValidationFailureException(PurchaseMessages.ItemInactive);

        var rate = line.Rate ?? purchasePrices.GetValueOrDefault(item.Id)?.PurchaseAmount ?? throw new ValidationFailureException(PurchaseMessages.NoPurchasePrice);
        return new VendorReturnLineInput(item.Id, item.Code, item.Name, line.StockSource!.Value, rate, line.Quantity!.Value);
    }
}
