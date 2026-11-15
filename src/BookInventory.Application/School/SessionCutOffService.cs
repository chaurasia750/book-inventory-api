using BookInventory.Application.Price;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.Domain.School;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.School;

/// <summary>R19: reads and changes the company's session cut-offs; the first change creates the company's row from the defaults.</summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class SessionCutOffService(ISessionSettingStore store, PriceCompanyGuard companyGuard)
{
    public async Task<SessionCutOffsResponse> GetAsync(CancellationToken cancellationToken) =>
        ResponseOf(await store.FindAsync(cancellationToken) ?? SessionSetting.WithDefaults());

    public async Task<SessionCutOffsResponse> ChangeAsync(ChangeSessionCutOffsRequest request, CancellationToken cancellationToken)
    {
        companyGuard.Require();
        var setting = await store.FindAsync(cancellationToken);
        if (setting is null)
        {
            setting = SessionSetting.WithDefaults();
            await store.AddAsync(setting, cancellationToken);
        }

        setting.Change(
            new SessionCutOff(request.AprMarMonth!.Value, request.AprMarDay!.Value),
            new SessionCutOff(request.JanDecMonth!.Value, request.JanDecDay!.Value));
        return ResponseOf(setting);
    }

    private static SessionCutOffsResponse ResponseOf(SessionSetting s) =>
        new(s.AprMarCutOffMonth, s.AprMarCutOffDay, s.JanDecCutOffMonth, s.JanDecCutOffDay);
}
