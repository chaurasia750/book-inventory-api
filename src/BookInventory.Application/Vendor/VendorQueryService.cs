using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.Domain.Vendor;
using Microsoft.Extensions.DependencyInjection;
using VendorRoot = BookInventory.Domain.Vendor.Vendor;
using BookInventory.Domain.Core;

namespace BookInventory.Application.Vendor;

/// <summary>R10: the vendor list and detail. The company comes from the platform filter; the roles are checked on the controller.</summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class VendorQueryService(IVendorStore store)
{
    public Task<IPagedList<VendorSummary>> SearchAsync(VendorSearchQuery query, CancellationToken cancellationToken) =>
        store.SearchAsync(query, cancellationToken);

    public async Task<VendorDetailResponse> GetAsync(Guid id, CancellationToken cancellationToken) =>
        VendorResponses.Detail(await store.FindForReadAsync(id, cancellationToken) ?? throw new NotFoundException(VendorMessages.NotFound));
}

/// <summary>The one projection of a vendor into its detail response, used by the create and the read use cases.</summary>
internal static class VendorResponses
{
    public static VendorDetailResponse Detail(VendorRoot vendor) =>
        new(
            vendor.Id, vendor.Code, vendor.Name, vendor.Address, vendor.ContactPerson, vendor.Mobile, vendor.Email, vendor.Gstin,
            vendor.State, Bank(vendor.Bank), vendor.OpeningBalanceAmount, vendor.Status, vendor.CreatedAt, vendor.UpdatedAt);

    private static VendorBankResponse? Bank(BankAccount? bank) =>
        bank is null ? null : new VendorBankResponse(bank.AccountHolder!, bank.BankName!, bank.Branch!, bank.AccountNumber!, bank.Ifsc!);
}
