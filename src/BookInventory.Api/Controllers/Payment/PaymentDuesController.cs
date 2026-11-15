using BookInventory.Application.Payment;
using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Payment;

/// <summary>Spec payment R3, R16: what a school owes and what is due on invoices, for the Owner and a Super Admin who entered the company.</summary>
[Route("payment")]
public sealed class PaymentDuesController(PaymentDues dues) : ApiControllerBase
{
    private const string Owners = "OWNER,SUPER_ADMIN";

    /// <summary>R3: the school's balance and open invoices, oldest first.</summary>
    [HttpGet("dues"), Authorize(Roles = Owners)]
    public async Task<IActionResult> OfSchool([FromQuery] Guid schoolId, CancellationToken cancellationToken) => Ok(await dues.OfSchoolAsync(schoolId, cancellationToken));

    /// <summary>R16: allocated and due amounts of up to 100 invoices.</summary>
    [HttpGet("invoice-dues"), Authorize(Roles = Owners)]
    public async Task<IActionResult> OfInvoices([FromQuery] Guid[] invoiceIds, CancellationToken cancellationToken) => Ok(await dues.OfInvoicesAsync(invoiceIds, cancellationToken));
}
