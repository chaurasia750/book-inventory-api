using System.Linq.Expressions;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.Iam;

/// <summary>
/// A person who can log in: staff (<c>SUPER_ADMIN</c>, <c>SALES_CRM</c>, <c>ACCOUNTS</c>), an agent
/// (<c>MEMBER</c>) or a company account (<c>OWNER</c>, <c>STAFF</c>, R59-R60). This is the aggregate root that owns the account lifecycle, the credentials and the lockout state;
/// nothing outside changes its properties, and every transition, uniqueness rule and reason requirement of R1-R6,
/// R28-R30, R34-R36 and R42 is enforced here.
/// <list type="bullet">
/// <item><b>Identity (R1).</b> Username, email and mobile are stored normalised (lower-case / digits only) so
/// uniqueness is a plain unique index. A rejected agent's email and mobile can be released (<see cref="ReleaseIdentity"/>),
/// so they are nullable.</item>
/// <item><b>Status (R28).</b> Only the transitions in the state diagram are allowed; anything else is a 409. Only
/// <see cref="AccountStatus.Active"/> may log in. A lockout is not a status but <see cref="LockedUntil"/>.</item>
/// <item><b>Super Admin (R2, R5).</b> Staff are created by a Super Admin with a forced password change. The last
/// <em>visible active</em> Super Admin cannot be deactivated, suspended or demoted; the caller passes the number of
/// other active visible Super Admins, because only a query can know it. The hidden system account is never counted.</item>
/// <item><b>System account (R42-R45).</b> <see cref="IsSystemAccount"/> is set by exactly one factory and no other
/// method may change anything about such an account.</item>
/// </list>
/// </summary>
public partial class AppUser : AggregateRoot
{
    /// <summary>Name of the EF query filter that hides the system account from every normal query (R43).</summary>
    public const string HiddenAccountFilterKey = "HiddenAccount";

    public const int ReasonMinLength = 5;
    public const int ReasonMaxLength = 500;

    private AppUser()
    {
        UserName = string.Empty;
        FullName = string.Empty;
        PasswordHash = string.Empty;
    }

    public string UserName { get; private set; }

    public string? Email { get; private set; }

    public string? Mobile { get; private set; }

    public string FullName { get; private set; }

    /// <summary>Hashed with PBKDF2-HMAC-SHA512 in a self-describing format (R16, D19). Never logged, never returned by a DTO.</summary>
    public string PasswordHash { get; private set; }

    public Role Role { get; private set; }

    public AccountStatus Status { get; private set; }

    /// <summary>Why the current status was set (reject/deactivate/suspend reason, or an approval note). Null when there is none.</summary>
    public string? StatusReason { get; private set; }

    public DateTime? StatusChangedAt { get; private set; }

    /// <summary>Who set the current status. Null means SYSTEM (an automatic suspension lift, R32).</summary>
    public Guid? StatusChangedBy { get; private set; }

    /// <summary>End of a suspension; after it passes the account lifts to <see cref="AccountStatus.Active"/> by itself (R32).</summary>
    public DateTime? SuspendedUntil { get; private set; }

    public bool MustChangePassword { get; private set; }

    /// <summary>The single hidden built-in Super Admin (R42). Set by <see cref="CreateSystemAccount"/> only.</summary>
    public bool IsSystemAccount { get; private set; }

    public int FailedLoginCount { get; private set; }

    public DateTime? LockedUntil { get; private set; }

    public DateTime? PasswordChangedAt { get; private set; }

    public DateTime? LastLoginAt { get; private set; }

    /// <summary>
    /// Counts towards the "at least one visible Super Admin" rule of R5. Written once as an expression, so the seeder and
    /// the directory query it with exactly the rule the aggregate applies to itself.
    /// </summary>
    public static readonly Expression<Func<AppUser, bool>> VisibleActiveSuperAdmin =
        u => u.Role == Role.SuperAdmin && u.Status == AccountStatus.Active && !u.IsSystemAccount;

    private static readonly Func<AppUser, bool> VisibleActiveSuperAdminRule = VisibleActiveSuperAdmin.Compile();

    public bool IsVisibleActiveSuperAdmin => VisibleActiveSuperAdminRule(this);

    public bool IsStaff => Role.IsStaff();

    public bool IsLockedAt(DateTime utcNow) => LockedUntil is { } until && until > utcNow;

    /// <summary>
    /// R3/R8: why the current status forbids a login, or <c>null</c> when it allows one. Only <c>ACTIVE</c> may log in; the
    /// answer names the specific reason, which the caller may reveal only after the password proved right (R8).
    /// </summary>
    public LoginResult? StatusRefusal => Status switch
    {
        AccountStatus.Active => null,
        AccountStatus.Pending => LoginResult.FailedPending,
        AccountStatus.Inactive => LoginResult.FailedInactive,
        AccountStatus.Suspended => LoginResult.FailedSuspended,
        _ => LoginResult.FailedRejected
    };

    // ---------------------------------------------------------------- status lifecycle

    /// <summary>R3/R28: an agent application is approved. The agent chose the password at registration, so no change is forced (owner decision).</summary>
    public void Approve(Guid actorId, DateTime utcNow, string? note = null)
    {
        EnsureNotSystemAccount();
        EnsureNotSelf(actorId);
        EnsureTransition(AccountStatus.Pending);

        SetStatus(AccountStatus.Active, note, suspendedUntil: null, new ChangeStamp(actorId, utcNow));
        AddDomainEvent(new UserActivated(Id, AccountStatus.Pending, LiftedAutomatically: false));
    }

    /// <summary>R28: <c>INACTIVE -&gt; ACTIVE</c> and <c>SUSPENDED -&gt; ACTIVE</c>. A pending account is approved, not activated.</summary>
    public void Activate(Guid actorId, DateTime utcNow, string? note = null)
    {
        EnsureNotSystemAccount();
        EnsureNotSelf(actorId);
        EnsureTransition(AccountStatus.Inactive, AccountStatus.Suspended);

        var from = Status;
        SetStatus(AccountStatus.Active, note, suspendedUntil: null, new ChangeStamp(actorId, utcNow));
        AddDomainEvent(new UserActivated(Id, from, LiftedAutomatically: false));
    }

    /// <summary>R28/R37: <c>ACTIVE -&gt; INACTIVE</c> and <c>SUSPENDED -&gt; INACTIVE</c>, the way to retire an account. Sessions are revoked by the handler.</summary>
    public void Deactivate(string reason, Guid actorId, DateTime utcNow, int otherActiveVisibleSuperAdmins)
    {
        EnsureNotSystemAccount();
        EnsureNotSelf(actorId);
        var validatedReason = RequireReason(reason);
        EnsureTransition(AccountStatus.Active, AccountStatus.Suspended);
        EnsureNotLastSuperAdmin(otherActiveVisibleSuperAdmins);

        SetStatus(AccountStatus.Inactive, validatedReason, suspendedUntil: null, new ChangeStamp(actorId, utcNow));
        AddDomainEvent(new UserDeactivated(Id, validatedReason));
    }

    /// <summary>R28/R30: <c>ACTIVE -&gt; SUSPENDED</c> with a mandatory reason and an optional future end date.</summary>
    public void Suspend(SuspensionRequest request, Guid actorId, DateTime utcNow, int otherActiveVisibleSuperAdmins)
    {
        EnsureNotSystemAccount();
        EnsureNotSelf(actorId);
        var validatedReason = RequireReason(request.Reason);
        var suspendedUntil = request.Until;
        if (suspendedUntil is { } until && until <= utcNow)
            throw new ValidationFailureException(IamMessages.UserSuspensionEndInPast);
        EnsureTransition(AccountStatus.Active);
        EnsureNotLastSuperAdmin(otherActiveVisibleSuperAdmins);

        SetStatus(AccountStatus.Suspended, validatedReason, suspendedUntil, new ChangeStamp(actorId, utcNow));
        AddDomainEvent(new UserSuspended(Id, validatedReason, suspendedUntil));
    }

    /// <summary>R28/R30: <c>PENDING -&gt; REJECTED</c>. Final (R58): the email and mobile stay reserved until released.</summary>
    public void Reject(string reason, Guid actorId, DateTime utcNow)
    {
        EnsureNotSystemAccount();
        EnsureNotSelf(actorId);
        EnsureTransition(AccountStatus.Pending);

        var validatedReason = RequireReason(reason);
        SetStatus(AccountStatus.Rejected, validatedReason, suspendedUntil: null, new ChangeStamp(actorId, utcNow));
        AddDomainEvent(new UserRejected(Id, validatedReason));
    }

    /// <summary>
    /// R32: a suspension whose end date has passed lifts to <see cref="AccountStatus.Active"/> by itself, recorded with
    /// actor <c>SYSTEM</c> (a null <see cref="StatusChangedBy"/>). Called by the periodic job and lazily at login, so the
    /// lift never depends on the job running. Returns <c>false</c> when there was nothing to lift.
    /// </summary>
    public bool LiftSuspensionIfDue(DateTime utcNow)
    {
        EnsureNotSystemAccount();

        if (Status != AccountStatus.Suspended || SuspendedUntil is not { } until || until > utcNow)
            return false;

        SetStatus(AccountStatus.Active, StatusReason, suspendedUntil: null, new ChangeStamp(Guid.Empty, utcNow));
        AddDomainEvent(new UserActivated(Id, AccountStatus.Suspended, LiftedAutomatically: true));
        return true;
    }

    // ---------------------------------------------------------------- role and profile

    /// <summary>
    /// R6: a Super Admin moves a user between the staff roles, with a reason, never their own account and never to or
    /// from <c>MEMBER</c>. The last visible Super Admin cannot be demoted.
    /// </summary>
    public void ChangeRole(RoleChange change, Guid actorId, DateTime utcNow, int otherActiveVisibleSuperAdmins)
    {
        EnsureNotSystemAccount();
        var newRole = change.NewRole;

        if (actorId == Id)
            throw new ValidationFailureException(IamMessages.UserSelfRoleChange);
        if (Role == Role.Member || newRole == Role.Member)
            throw new ValidationFailureException(IamMessages.UserMemberRoleImmutable);
        if (Role.IsCompanyRole() || newRole.IsCompanyRole())
            throw new ValidationFailureException(IamMessages.UserCompanyRoleImmutable);
        if (!newRole.IsStaff())
            throw new ValidationFailureException(IamMessages.UserRoleNotStaff);
        if (newRole == Role)
            throw new ConflictException(IamMessages.UserRoleUnchanged);
        if (Role == Role.SuperAdmin && newRole != Role.SuperAdmin)
            EnsureNotLastSuperAdmin(otherActiveVisibleSuperAdmins);

        var validatedReason = RequireReason(change.Reason, IamMessages.UserRoleChangeReasonRequired);

        var from = Role;
        Role = newRole;
        AddDomainEvent(new UserRoleChanged(Id, from, newRole, validatedReason));
    }

    /// <summary>
    /// R35: a Super Admin edits somebody else's full name, email and mobile. User name and role never change here, an
    /// e-mail that is not supplied keeps the stored one, and the aggregate is the only place that decides the shape and
    /// the length of a value.
    /// <para>
    /// A Super Admin editing <em>their own</em> record is refused: R35 makes an own e-mail or mobile change conditional
    /// on the current password, and that password only exists on the self-service endpoint (<c>PUT auth/profile</c>).
    /// Without this guard the admin endpoint would be a way around that password.
    /// </para>
    /// </summary>
    public void UpdateProfile(string fullName, string? email, string? mobile, Guid actorId)
    {
        EnsureNotSystemAccount();
        EnsureNoSelfAction(actorId);
        ApplyProfile(fullName, email, mobile);
    }

    /// <summary>
    /// R41/R35: a user edits their own full name, e-mail and mobile. Whether the <em>current password</em> the change of an
    /// e-mail or a mobile needs was given is checked by the use case (only it can verify a password); the shape, the length
    /// and the "not the system account" rule are the aggregate's. The user name and the role never change here.
    /// </summary>
    public void UpdateOwnProfile(string fullName, string? email, string? mobile)
    {
        EnsureNotSystemAccount();
        ApplyProfile(fullName, email, mobile);
    }

    private void ApplyProfile(string fullName, string? email, string? mobile)
    {
        FullName = RequireFullName(fullName);
        if (email is not null)
            Email = NormalizeEmail(email);
        if (mobile is not null)
            Mobile = NormalizeMobile(mobile);
    }

    /// <summary>R35: whether an own e-mail or mobile would change, which is what makes the current password mandatory.</summary>
    public bool WouldChangeContact(string? email, string? mobile) =>
        (email is not null && NormalizeEmail(email) != Email) || (mobile is not null && NormalizeMobile(mobile) != Mobile);

    /// <summary>
    /// R58: a Super Admin frees the email and mobile of a rejected agent so a new application can use them. The account
    /// itself stays as history.
    /// </summary>
    public void ReleaseIdentity(Guid actorId, DateTime utcNow)
    {
        EnsureNotSystemAccount();

        if (actorId == Id)
            throw new ValidationFailureException(IamMessages.UserSelfActionForbidden);
        if (Status != AccountStatus.Rejected)
            throw new ConflictException(IamMessages.UserInvalidTransition);

        Email = null;
        Mobile = null;
    }

    // ---------------------------------------------------------------- credentials and lockout

    /// <summary>
    /// R18/R19: sets a new password, clears the forced change, resets the lockout counter and raises the event whose
    /// handler revokes the other sessions of the user.
    /// </summary>
    public void SetPassword(string passwordHash, DateTime utcNow)
    {
        EnsureNotSystemAccount();

        PasswordHash = RequirePasswordHash(passwordHash);
        PasswordChangedAt = utcNow;
        MustChangePassword = false;
        FailedLoginCount = 0;
        LockedUntil = null;

        AddDomainEvent(new UserPasswordChanged(Id, ForcedChange: false));
    }

    /// <summary>
    /// R16: re-hashes a password that was stored with weaker parameters than the current policy, during a successful
    /// login. Deliberately has no side effect beyond the hash - unlike <see cref="SetPassword"/> it changes no flag,
    /// raises no event and revokes no session, because the user did not change their password here.
    /// </summary>
    public void UpgradePasswordHash(string passwordHash) => PasswordHash = RequirePasswordHash(passwordHash);

    /// <summary>R36: a Super Admin forces a password change on the next login.</summary>
    public void RequirePasswordChange()
    {
        EnsureNotSystemAccount();
        MustChangePassword = true;
    }

    /// <summary>
    /// R9: one more wrong password. Returns <c>true</c> when this attempt is the one that locks the account. An account
    /// that is already locked is not counted and its lock is not extended - the caller answers "locked" instead.
    /// </summary>
    public bool RecordFailedLogin(DateTime utcNow, LockoutPolicy policy)
    {
        if (IsLockedAt(utcNow))
            return true;

        FailedLoginCount++;
        if (FailedLoginCount < policy.MaxAttempts)
            return false;

        LockedUntil = utcNow.Add(policy.LockoutDuration);
        return true;
    }

    /// <summary>R9: a success clears the counter and the lock and stamps the last login.</summary>
    public void RecordSuccessfulLogin(DateTime utcNow)
    {
        FailedLoginCount = 0;
        LockedUntil = null;
        LastLoginAt = utcNow;
    }

    /// <summary>R34: a Super Admin clears a lockout before it expires.</summary>
    public void Unlock(Guid actorId)
    {
        EnsureNotSystemAccount();
        EnsureNoSelfAction(actorId);

        FailedLoginCount = 0;
        LockedUntil = null;
    }

}
