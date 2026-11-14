using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Accounts;
using BookInventory.Application.Iam.Companies;
using BookInventory.Application.Iam.Passwords;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Staff;

/// <summary>
/// R65/R57: an Owner creates a Staff login for their own company. The company is the one of the Owner's session (R62),
/// never a request field; the user id is the Owner's id plus the short name; the password is generated and shown once.
/// Only the Staff role can come out of this, because the aggregate has no other factory for a company account.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class StaffCreationService(
    IAccountStore store,
    UserIdentityService identity,
    TemporaryPasswordIssuer passwords,
    ICurrentUser currentUser,
    ICurrentCompany currentCompany,
    TimeProvider timeProvider)
{
    public async Task<CreatedCompanyStaffResponse> CreateAsync(AddStaffRequest request, CancellationToken cancellationToken)
    {
        var companyId = currentCompany.CompanyId ?? throw new ForbiddenException(IamMessages.UserCompanyInvalid);
        var ownerUserName = currentUser.UserName ?? throw new UnauthenticatedException(IamMessages.SessionNotFound);
        var userId = StaffUserId.From(ownerUserName, request.ShortName);

        await identity.EnsureAvailableAsync(new UserIdentity(userId, Email: null, Mobile: null), cancellationToken);

        var password = passwords.Issue();
        var staff = AppUser.CreateCompanyStaff(
            new UserDraft(request.FullName, userId, Email: null, Mobile: null, password.Hash),
            companyId,
            currentUser.UserId,
            timeProvider.GetUtcNow().UtcDateTime);

        await store.AddAsync(staff, cancellationToken);
        return new CreatedCompanyStaffResponse(staff.Id, staff.UserName, staff.FullName, password.Plain);
    }
}
