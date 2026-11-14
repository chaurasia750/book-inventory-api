using System.ComponentModel.DataAnnotations;

namespace BookInventory.Application.Iam.Staff;

/// <summary>R65: what an Owner gives for a new Staff login. The user id is built from the Owner's id and the short name, never typed in full.</summary>
public sealed record AddStaffRequest(
    [param: Required] string FullName,
    [param: Required] string ShortName);

/// <summary>R65/R57: the new Staff login. The temporary password is shown once and never retrievable again.</summary>
public sealed record CreatedCompanyStaffResponse(Guid Id, string UserName, string FullName, string TemporaryPassword);
