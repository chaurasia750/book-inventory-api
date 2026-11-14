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
}
