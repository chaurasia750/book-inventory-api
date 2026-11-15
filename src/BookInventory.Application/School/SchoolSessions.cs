using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.Domain.School;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.School;

/// <summary>R18, R19: the school's session type and the company's cut-off (or the default) through the one rule of <see cref="SchoolSession"/>.</summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ISchoolSessions))]
public sealed class SchoolSessions(ISchoolStore schools, ISessionSettingStore settings) : ISchoolSessions
{
    public async Task<SessionChoice?> SessionOfAsync(Guid schoolId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var school = await schools.FindForReadAsync(schoolId, cancellationToken);
        if (school is null)
            return null;

        var setting = await settings.FindAsync(cancellationToken) ?? SessionSetting.WithDefaults();
        return SchoolSession.Of(school.SessionType, date, setting.CutOffOf(school.SessionType));
    }
}
