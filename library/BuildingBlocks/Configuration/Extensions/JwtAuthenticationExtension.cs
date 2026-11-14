using System.ComponentModel.DataAnnotations;
using System.Text;
using BookInventory.BuildingBlocks.Configuration.Models;
using BookInventory.BuildingBlocks.Configuration.Services;
using BookInventory.BuildingBlocks.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace BookInventory.BuildingBlocks.Configuration.Extensions
{
    public static class JwtAuthenticationExtension
    {
        /// <summary>
        /// JWT bearer authentication. Token lookup order: <c>Authorization: Bearer</c> header, HttpOnly cookie
        /// (<c>AuthCookie:AccessTokenCookieName</c>, default <c>accessToken</c>), form field <c>access_token</c>.
        /// Claims keep their wire names (<c>sub</c>, <c>role</c>, <c>sid</c>, <c>name</c>). Also registers
        /// <see cref="ICurrentUser"/> for the audit columns. Invalid <c>JwtSetting</c> values stop the application at startup.
        /// </summary>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = configuration.GetSection(nameof(JwtSetting)).Get<JwtSetting>() ?? new JwtSetting();
            try
            {
                Validator.ValidateObject(settings, new ValidationContext(settings), validateAllProperties: true);
            }
            catch (ValidationException ex)
            {
                throw new InvalidOperationException($"Invalid {nameof(JwtSetting)} configuration: {ex.Message}", ex);
            }

            var key = Encoding.UTF8.GetBytes(settings.IssuerSigningKey);
            var accessTokenCookieName = configuration["AuthCookie:AccessTokenCookieName"] ?? "accessToken";

            services.AddHttpContextAccessor();
            services.AddSingleton(settings);
            services.AddScoped<ICurrentUser, HttpCurrentUser>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = settings.RequireHttpsMetadata;
                options.SaveToken = true;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = settings.ValidateIssuerSigningKey,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = settings.ValidateIssuer,
                    ValidIssuer = settings.ValidIssuer,
                    ValidateAudience = settings.ValidateAudience,
                    ValidAudience = settings.ValidAudience,
                    RequireExpirationTime = settings.RequireExpirationTime,
                    ValidateLifetime = settings.ValidateLifetime,
                    ClockSkew = settings.ClockSkew,
                    NameClaimType = AppClaimTypes.Name,
                    RoleClaimType = AppClaimTypes.Role
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = async context =>
                    {
                        var authorization = context.Request.Headers.Authorization.ToString();
                        if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Token = authorization["Bearer ".Length..].Trim();
                            return;
                        }

                        if (context.Request.Cookies.TryGetValue(accessTokenCookieName, out var cookieToken)
                            && !string.IsNullOrEmpty(cookieToken))
                        {
                            context.Token = cookieToken;
                            return;
                        }

                        if (context.Request.HasFormContentType)
                        {
                            var form = await context.Request.ReadFormAsync(context.HttpContext.RequestAborted);
                            if (form.TryGetValue("access_token", out var formToken))
                                context.Token = formToken;
                        }
                    }
                };
            });

            return services;
        }
    }
}
