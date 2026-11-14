using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Accounts;

/// <summary>
/// The persistence port of the Super Admin account management. It exists next to
/// <c>IAuthenticationStore</c> because these use cases need three things the login use cases do not: the <em>tracked</em>
/// aggregate they are about to change, the two append-only history roots of R6/R33, and the uniqueness lookups of R1.
/// <para>
/// Like <c>IAuthenticationStore</c> it has no save method: a write request is committed by the request transaction
/// (<c>TransactionFilter</c>), and the one use case outside HTTP (the suspension lift) commits through
/// <c>IUnitOfWork</c> itself.
/// </para>
/// </summary>
public interface IAccountStore
{
    /// <summary>
    /// R42/R43: the account a Super Admin is about to <em>change</em>. The hidden system account is <em>not</em> found, so
    /// every command against its id answers the same 404 as an unknown id (owner decision) and its existence
    /// never shows. The aggregate's own guard (403) stays as a second line of defence for code that reaches it another way.
    /// </summary>
    Task<AppUser?> FindForManagementAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(AppUser user, CancellationToken cancellationToken = default);

    /// <summary>
    /// R33: appends one row of the status timeline. Its own root and insert-only, so it is never updated; the aggregate
    /// that changed the status and the row that records it are saved by the same request transaction.
    /// </summary>
    Task AddAsync(UserStatusHistory history, CancellationToken cancellationToken = default);

    /// <summary>R67: appends the record of one password reset by a person (who and when, never a password); insert-only.</summary>
    Task AddAsync(UserPasswordReset reset, CancellationToken cancellationToken = default);

    /// <summary>R6/AC24: appends one row of the role timeline; insert-only, like the status timeline.</summary>
    Task AddAsync(UserRoleHistory history, CancellationToken cancellationToken = default);

    /// <summary>R33: the append-only status timeline of one user, newest first.</summary>
    Task<UserStatusHistory[]> FindStatusHistoryAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// R48: the accounts behind the <c>changed_by</c> ids of a status timeline, so it can show who acted. This one
    /// lookup deliberately includes the hidden system account, because it can be the actor (R44/R49) and is then
    /// reported without a name. Only the id, the name and the flag are ever used.
    /// </summary>
    Task<AppUser[]> FindActorsAsync(Guid[] userIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// R1: whether the normalised user name is already taken. <paramref name="excludingUserId"/> is the account being
    /// edited, so saving a form that leaves a value unchanged is not a duplicate of itself.
    /// </summary>
    Task<bool> IsUserNameTakenAsync(string normalizedUserName, Guid? excludingUserId, CancellationToken cancellationToken = default);

    /// <summary>R1: whether the normalised e-mail is already taken, ignoring the account being edited.</summary>
    Task<bool> IsEmailTakenAsync(string normalizedEmail, Guid? excludingUserId, CancellationToken cancellationToken = default);

    /// <summary>R1: whether the normalised mobile is already taken, ignoring the account being edited.</summary>
    Task<bool> IsMobileTakenAsync(string normalizedMobile, Guid? excludingUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// R32: the accounts whose suspension has run out, oldest end date first and at most <paramref name="take"/>, for
    /// the lift job. The status is part of the predicate, so a user that was deactivated in the meantime is not touched.
    /// </summary>
    Task<AppUser[]> FindDueSuspensionsAsync(DateTime now, int take, CancellationToken cancellationToken = default);
}
