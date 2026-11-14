using BookInventory.BuildingBlocks.Configuration.Extensions;
using BookInventory.BuildingBlocks.Domain;
using System.Security.Claims;

namespace BookInventory.BuildingBlocks.Configuration.Services
{
    /// <summary><see cref="ICurrentUser"/> backed by the JWT claims of the current HTTP request.</summary>
    internal sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
    {
        private ClaimsPrincipal? User => accessor.HttpContext?.User;

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
        public Guid UserId => User?.GetUserId() ?? Guid.Empty;
        public string? UserName => User?.GetUserName();
        public string? Role => User?.GetRole();
        public Guid? SessionId => User?.GetSessionId();
    }
}
