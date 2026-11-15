using BookInventory.Application.School;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.Domain.School;
using Microsoft.EntityFrameworkCore;

namespace BookInventory.Infrastructure.School;

/// <summary>R19: the company's cut-off row, through the company-filtered repository.</summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ISessionSettingStore))]
public sealed class SessionSettingStore(IUnitOfWork unitOfWork) : ISessionSettingStore
{
    private readonly IRepository<SessionSetting> _settings = unitOfWork.Repository<SessionSetting>();

    public Task AddAsync(SessionSetting setting, CancellationToken cancellationToken = default) => _settings.AddAsync(setting, cancellationToken);

    public Task<SessionSetting?> FindAsync(CancellationToken cancellationToken = default) => _settings.Table.FirstOrDefaultAsync(cancellationToken);
}
