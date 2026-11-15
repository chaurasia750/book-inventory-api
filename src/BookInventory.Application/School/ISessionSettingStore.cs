using BookInventory.Domain.School;

namespace BookInventory.Application.School;

/// <summary>R19: the persistence port of the company's session cut-offs (one row per company, company-filtered).</summary>
public interface ISessionSettingStore
{
    Task AddAsync(SessionSetting setting, CancellationToken cancellationToken = default);

    /// <summary>The company's row, tracked, or <c>null</c> while the defaults apply.</summary>
    Task<SessionSetting?> FindAsync(CancellationToken cancellationToken = default);
}
