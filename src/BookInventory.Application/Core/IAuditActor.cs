using BookInventory.Domain.Core;

namespace BookInventory.Application.Core;

/// <summary>
/// Who is making the change being saved (spec core A4): the signed-in user with their id and user name, or "System" for a
/// background job, an anonymous request and the hidden system account. A port, so the persistence layer never reads
/// claims; the API host implements it from the access token.
/// </summary>
public interface IAuditActor
{
    AuditActorInfo Current { get; }
}
