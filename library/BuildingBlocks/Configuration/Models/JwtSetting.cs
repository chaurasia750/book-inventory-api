using System.ComponentModel.DataAnnotations;

namespace BookInventory.BuildingBlocks.Configuration.Models
{
    /// <summary>Bound from the <c>JwtSetting</c> configuration section and validated at startup.</summary>
    public class JwtSetting
    {
        public bool ValidateIssuerSigningKey { get; init; } = true;

        /// <summary>HMAC secret, at least 32 characters. Keep it in a secret store, never in appsettings.json.</summary>
        [Required, MinLength(32)]
        public string IssuerSigningKey { get; init; } = string.Empty;

        public bool ValidateIssuer { get; init; } = true;
        public string ValidIssuer { get; init; } = string.Empty;
        public bool ValidateAudience { get; init; } = true;
        public string ValidAudience { get; init; } = string.Empty;
        public bool RequireExpirationTime { get; init; } = true;
        public bool ValidateLifetime { get; init; } = true;

        /// <summary>Tolerated clock difference between token issuer and this API (0 to 5 minutes).</summary>
        [Range(typeof(TimeSpan), "00:00:00", "00:05:00")]
        public TimeSpan ClockSkew { get; init; } = TimeSpan.FromMinutes(1);

        /// <summary>Set false only for local development against an http identity provider.</summary>
        public bool RequireHttpsMetadata { get; init; } = true;
    }
}
