using BookInventory.Application.Core;
using BookInventory.Application.Iam.Companies;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.BuildingBlocks.Persistence.Extensions;
using BookInventory.Domain.Core;
using Microsoft.EntityFrameworkCore;

namespace BookInventory.Infrastructure.Core;

/// <summary>
/// Spec platform P23: the company code series. The row of (company, prefix) is created on first use and increased under a
/// PostgreSQL advisory lock keyed on that pair, held until the request transaction ends, so parallel creations of one
/// company and prefix are serialised while other companies and prefixes do not wait.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ICompanyCodes))]
public sealed class CompanyCodes(IUnitOfWork unitOfWork, ICurrentCompany currentCompany) : ICompanyCodes
{
    /// <summary>Arbitrary constant namespace of the advisory lock, combined with the hash of company and prefix.</summary>
    private const int LockNamespace = 5_200_002;

    private readonly IRepository<CompanyCodeSeries> _series = unitOfWork.Repository<CompanyCodeSeries>();

    public async Task<string> NextAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var companyId = currentCompany.CompanyId ?? Guid.Empty;
        var normalizedPrefix = prefix.Trim().ToUpperInvariant();
        var key = HashCode.Combine(companyId, normalizedPrefix);
        await _series.ExecuteSqlAsync($"select pg_advisory_xact_lock({LockNamespace}, {key})", cancellationToken);

        var series = await _series.Table.FirstOrDefaultAsync(s => s.Prefix == normalizedPrefix, cancellationToken);
        if (series is null)
        {
            series = CompanyCodeSeries.Start(normalizedPrefix);
            await _series.AddAsync(series, cancellationToken);
        }

        return series.Next();
    }
}
