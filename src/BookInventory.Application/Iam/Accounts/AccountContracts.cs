using System.ComponentModel.DataAnnotations;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Accounts;

/// <summary>
/// R2/R57: what a Super Admin supplies to create a staff account. The password is <em>not</em> part of it - the system
/// generates a temporary one and shows it once (R57).
/// <para>
/// The bounds are data annotations so an oversized value is refused at the boundary (400 with a
/// <c>ValidationProblemDetails</c>). Every other rule - the character set of the user name, the shape of the e-mail and
/// the mobile, the uniqueness of all three (R1), the role being a staff role (R2) - belongs to the aggregate, so it is
/// answered with a catalogue code and exists in exactly one place.
/// </para>
/// </summary>
public sealed record CreateStaffRequest(
    [param: Required, StringLength(IdentityNormalizer.FullNameMaxLength, MinimumLength = 1)] string FullName,
    [param: Required, StringLength(IdentityNormalizer.UsernameMaxLength, MinimumLength = IdentityNormalizer.UsernameMinLength)] string UserName,
    [param: Required, StringLength(IdentityNormalizer.EmailMaxLength, MinimumLength = 3)] string Email,
    [param: StringLength(IdentityNormalizer.MobileMaxLength, MinimumLength = IdentityNormalizer.MobileMinLength)] string? Mobile,
    [param: Required] Role? Role);

/// <summary>
/// R35: the fields a Super Admin may change on somebody else's account. The user name and the role are deliberately
/// absent - R35 keeps them fixed, and the role has its own endpoint with a reason (R6). E-mail and mobile that are left
/// out keep the stored value, so an edit cannot silently erase an address.
/// </summary>
public sealed record UpdateUserRequest(
    [param: Required, StringLength(IdentityNormalizer.FullNameMaxLength, MinimumLength = 1)] string FullName,
    [param: StringLength(IdentityNormalizer.EmailMaxLength, MinimumLength = 3)] string? Email,
    [param: StringLength(IdentityNormalizer.MobileMaxLength, MinimumLength = IdentityNormalizer.MobileMinLength)] string? Mobile);

/// <summary>
/// R30: the body of the five status transitions. <c>Reason</c> is mandatory for reject, deactivate and suspend and
/// optional for approve and activate - that difference is the aggregate's rule, not a data annotation, so the catalogue
/// code is what comes back. <c>SuspendedUntil</c> only means anything on <c>suspend</c> (R32) and must then be in the
/// future.
/// </summary>
public sealed record StatusChangeRequest(
    [param: StringLength(AppUser.ReasonMaxLength)] string? Reason = null,
    DateTime? SuspendedUntil = null);

/// <summary>R6: the new staff role and the mandatory reason for the change.</summary>
public sealed record ChangeRoleRequest(
    [param: Required] Role? Role,
    [param: StringLength(AppUser.ReasonMaxLength)] string? Reason = null);

/// <summary>
/// The three values R1 makes unique, as they arrive - still un-normalised. A <c>null</c> means "not supplied", so an edit
/// that leaves an address out keeps the stored one instead of clearing it. Normalisation is
/// <see cref="IdentityNormalizer"/>'s, the same call the aggregate makes, and a value that is not valid at all never
/// reaches a lookup: the aggregate rejects it first when it builds or changes the account.
/// </summary>
public sealed record UserIdentity(string? UserName, string? Email, string? Mobile);
