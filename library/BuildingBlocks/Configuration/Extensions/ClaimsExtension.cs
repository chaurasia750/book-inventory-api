using System.Security.Claims;

namespace BookInventory.BuildingBlocks.Configuration.Extensions
{
    /// <summary>The standard claim names every token uses. Role names themselves belong to the application.</summary>
    public static class AppClaimTypes
    {
        public const string Subject = "sub";
        public const string Name = "name";
        public const string Role = "role";
        public const string SessionId = "sid";
    }

    /// <summary>Typed claim readers. A missing or malformed claim yields <see cref="Guid.Empty"/> / null, never an exception.</summary>
    public static class ClaimsExtension
    {
        public static Guid GetUserId(this ClaimsPrincipal user) => ReadGuid(user, AppClaimTypes.Subject) ?? Guid.Empty;

        public static Guid? GetSessionId(this ClaimsPrincipal user) => ReadGuid(user, AppClaimTypes.SessionId);

        public static string? GetUserName(this ClaimsPrincipal user) => user.FindFirstValue(AppClaimTypes.Name);

        public static string? GetRole(this ClaimsPrincipal user) => user.FindFirstValue(AppClaimTypes.Role);

        private static Guid? ReadGuid(ClaimsPrincipal user, string type) =>
            Guid.TryParse(user.FindFirstValue(type), out var value) ? value : null;
    }
}
