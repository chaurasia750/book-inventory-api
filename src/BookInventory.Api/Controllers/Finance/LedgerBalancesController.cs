using BookInventory.Application.Finance;
using BookInventory.BuildingBlocks.Configuration.Controller;
using BookInventory.Domain.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Finance;

/// <summary>Spec finance R17: the balances of the schools or vendors shown on a list page, for the Owner and an entered Super Admin.</summary>
[Route("ledger/balances"), Authorize(Roles = Readers)]
public sealed class LedgerBalancesController(LedgerStatementService statements) : ApiControllerBase
{
    private const string Readers = "OWNER,SUPER_ADMIN";

    /// <summary>R17: the balances of the listed schools (at most 100 ids).</summary>
    [HttpGet("schools")]
    public async Task<IActionResult> Schools([FromQuery] Guid[] ids, CancellationToken cancellationToken) =>
        Ok(await statements.BalancesAsync(PartyType.School, ids, cancellationToken));

    /// <summary>R17: the balances of the listed vendors (at most 100 ids).</summary>
    [HttpGet("vendors")]
    public async Task<IActionResult> Vendors([FromQuery] Guid[] ids, CancellationToken cancellationToken) =>
        Ok(await statements.BalancesAsync(PartyType.Vendor, ids, cancellationToken));
}
