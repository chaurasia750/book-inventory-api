using System.Linq.Expressions;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using BookInventory.Application.Iam.Companies;
using BookInventory.Domain.Core;

namespace BookInventory.Infrastructure.Persistence;

/// <summary>
/// Spec platform P19/P20: the two halves of company-owned data. The named query filter <see cref="FilterKey"/> limits every
/// read of an <see cref="ICompanyOwned"/> entity to the company of the session; with no company it compares with an empty
/// id, so nothing matches, never everything. Before a save, every added company-owned row gets the session's company; with
/// no company the save is refused. Code that must see across companies says so with <c>IgnoreQueryFilters([CompanyScope.FilterKey])</c>.
/// <para>
/// EF Core builds the model once and caches it for every context instance, so the filter must not capture one instance.
/// It reads <c>AppDbContext.SessionCompanyId</c> through the context <em>parameter</em> of the filter expression, which EF
/// binds to the instance running the query (the documented way to write a per-request filter).
/// </para>
/// </summary>
internal static class CompanyScope
{
    public const string FilterKey = "Company";

    /// <summary>P19: adds the filter to every company-owned entity type of the model.</summary>
    public static void ApplyFilter(ModelBuilder modelBuilder, AppDbContext context)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(t => typeof(ICompanyOwned).IsAssignableFrom(t.ClrType) && t.BaseType is null).ToList())
        {
            var filter = (LambdaExpression)typeof(CompanyScope)
                .GetMethod(nameof(FilterFor), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .MakeGenericMethod(entityType.ClrType)
                .Invoke(null, [context])!;
            entityType.SetQueryFilter(FilterKey, filter);
        }
    }

    /// <summary>
    /// The lambda captures the <paramref name="context"/> variable, which EF rewrites into "the context instance that runs
    /// this query"; its property is therefore read per query, not frozen into the cached model.
    /// </summary>
    private static Expression<Func<T, bool>> FilterFor<T>(AppDbContext context) where T : class, ICompanyOwned =>
        e => e.CompanyId == context.SessionCompanyId;

    /// <summary>P20: stamps the session's company on every added company-owned row; refuses when the session has none.</summary>
    public static void StampAddedRows(ChangeTracker tracker, ICurrentCompany currentCompany)
    {
        var added = tracker.Entries<DomainBase>()
            .Where(e => e.State == EntityState.Added && e.Entity is ICompanyOwned)
            .Select(e => (ICompanyOwned)e.Entity)
            .ToList();
        if (added.Count == 0)
            return;

        var companyId = currentCompany.CompanyId ?? throw new ForbiddenException(CoreMessages.CompanyRequired);
        foreach (var row in added)
            row.AssignCompany(companyId);
    }
}
