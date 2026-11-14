namespace BookInventory.BuildingBlocks.Domain
{
    /// <summary>
    /// The user on whose behalf the current operation runs. Implemented by the API host (from JWT claims) and consumed
    /// by persistence for the audit columns. Contains no role constants: role names belong to the application.
    /// </summary>
    public interface ICurrentUser
    {
        /// <summary><see cref="Guid.Empty"/> when there is no authenticated user (background jobs, seeding).</summary>
        Guid UserId { get; }

        string? UserName { get; }

        /// <summary>The role name from the token (for example <c>SUPER_ADMIN</c>), or null.</summary>
        string? Role { get; }

        /// <summary>The login session the token belongs to, or null.</summary>
        Guid? SessionId { get; }

        bool IsAuthenticated { get; }
    }
}
