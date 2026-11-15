namespace BookInventory.Application.School;

/// <summary>R16: what another module may know about a school: its id, code, name and whether it can be chosen for a new document.</summary>
public sealed record SchoolReference(Guid Id, string Code, string Name, bool IsActive, string? State = null, bool IsGstDefault = false, string? Gstin = null);

/// <summary>
/// R16: the capability other modules (the ledger and the customer master) use to read schools, never by joining the school
/// tables. It answers only for the company of the session (platform P19), returns plain values and never the aggregate, and
/// changes nothing.
/// </summary>
public interface ISchoolDirectory
{
    /// <summary>The school's id, code, name and active flag, or <c>null</c> for an unknown id or another company's.</summary>
    Task<SchoolReference?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>The references of the given schools, by id; an id that is not found is left out.</summary>
    Task<IReadOnlyDictionary<Guid, SchoolReference>> ReferencesAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>The school's own class name that equals <paramref name="text"/> ignoring case and extra spaces (R6), or <c>null</c>.</summary>
    Task<string?> ClassOfAsync(Guid schoolId, string? text, CancellationToken cancellationToken = default);

    /// <summary>The school's class names in the school's order (R6), empty for an unknown school (for planning R2, R4, R7).</summary>
    Task<IReadOnlyList<string>> ClassesOfAsync(Guid schoolId, CancellationToken cancellationToken = default);
}
