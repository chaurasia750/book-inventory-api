using System.Text;
using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.Company;

/// <summary>
/// R4/D4: the login of a company's Owner, generated from the company's name: ASCII letters and digits only, lower
/// case, spaces and symbols dropped, cut to 32 characters ("Children Book Center" gives <c>childrenbookcenter</c>).
/// A name that leaves fewer than 3 characters is refused, because <c>iam</c> user ids need at least 3. Whether the id is
/// free is <c>iam</c>'s check (R1); a taken id answers 409 and the Super Admin changes the name.
/// </summary>
public static class CompanyOwnerUserId
{
    public const int MinLength = 3;
    public const int MaxLength = 32;

    public static string From(string companyName)
    {
        var id = new StringBuilder();
        foreach (var c in companyName.ToLowerInvariant())
        {
            if (id.Length < MaxLength && c is (>= 'a' and <= 'z') or (>= '0' and <= '9'))
                id.Append(c);
        }

        return id.Length >= MinLength ? id.ToString() : throw new ValidationFailureException(CompanyMessages.OwnerIdInvalid);
    }
}
