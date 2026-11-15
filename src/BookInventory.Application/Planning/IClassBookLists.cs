namespace BookInventory.Application.Planning;

/// <summary>Spec planning R2, R3 (for sales R22): one book of a class list: the item and how many copies each student needs.</summary>
public sealed record ClassBookReference(Guid ItemId, string ItemCode, int QuantityPerStudent);

/// <summary>Spec planning R2, R3 (for sales R22): one class's list: its class name, student estimate and books.</summary>
public sealed record ClassBookListReference(string ClassName, int? StudentEstimate, IReadOnlyList<ClassBookReference> Books);

/// <summary>
/// The capability other modules (sales R22 first) use to read class book lists, never by joining the planning tables. It answers
/// only for the session's company, returns plain values and changes nothing.
/// </summary>
public interface IClassBookLists
{
    /// <summary>The lists of a school's session, keyed by the class as the list writes it (compared ignoring case and extra spaces).</summary>
    Task<IReadOnlyDictionary<string, ClassBookListReference>> ListsOfAsync(Guid schoolId, string session, CancellationToken cancellationToken = default);
}
