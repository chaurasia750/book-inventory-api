using BookInventory.Application.Finance;
using BookInventory.BuildingBlocks.Configuration.Controller;
using BookInventory.Domain.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Finance;

/// <summary>Spec finance R11, R13: the outstanding lists of all schools and all vendors, for the Owner and an entered Super Admin.</summary>
[Route("ledger/outstanding"), Authorize(Roles = Readers)]
public sealed class OutstandingController(LedgerStatementService statements) : ApiControllerBase
{
    private const string Readers = "OWNER,SUPER_ADMIN";

    /// <summary>R11: schools with a non-zero balance, largest first, with the total.</summary>
    [HttpGet("schools")]
    public async Task<IActionResult> Schools([FromQuery] OutstandingQuery query, CancellationToken cancellationToken) =>
        Ok(await statements.OutstandingAsync(PartyType.School, query, cancellationToken));

    /// <summary>R11: vendors with a non-zero balance, largest first, with the total.</summary>
    [HttpGet("vendors")]
    public async Task<IActionResult> Vendors([FromQuery] OutstandingQuery query, CancellationToken cancellationToken) =>
        Ok(await statements.OutstandingAsync(PartyType.Vendor, query, cancellationToken));
}
