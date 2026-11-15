namespace BookInventory.Application.Core;

/// <summary>
/// Spec platform P23: hands out the generated codes of the session's company (<c>SCH-0001</c>, <c>VEN-0001</c>, ...). A
/// module asks for its own prefix and never keeps a counter of its own. The number is taken under a lock that lasts until the
/// request transaction ends, so parallel creations queue behind each other and get different numbers; a refused creation
/// rolls the number back.
/// </summary>
public interface ICompanyCodes
{
    Task<string> NextAsync(string prefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// P23: the next number of a series (at most 20 characters, for example <c>ORD/2026-27/</c>), under the same lock and in the same
    /// transaction as <see cref="NextAsync"/>; the document module formats the code itself.
    /// </summary>
    Task<int> NextNumberAsync(string series, CancellationToken cancellationToken = default);
}
