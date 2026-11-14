using System.Reflection;
using BookInventory.BuildingBlocks.Application.Extensions;
using BookInventory.BuildingBlocks.Configuration.Extensions;
using BookInventory.BuildingBlocks.Logger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using BookInventory.Api.Auth;
using BookInventory.Api.Binding;
using BookInventory.Api.Hosting;
using BookInventory.Application;
using BookInventory.Application.Iam;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Application.Iam.Registration;
using BookInventory.Application.Core;
using BookInventory.Infrastructure;
using BookInventory.Infrastructure.Core.Configuration;
using BookInventory.Infrastructure.Iam;
using BookInventory.Infrastructure.Iam.Configuration;
using BookInventory.Infrastructure.Persistence.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddLogger();

builder.Services
    .AddApiBuildingBlocks(builder.Configuration)   // MVC filters, JWT, CORS, versioning, OpenAPI, health
    .AddApplication()
    .AddInfrastructure(builder.Configuration);     // DbContext (PostgreSQL), unit of work, repositories

// R54: every tunable security value is bound and validated here, so an invalid one stops startup (AC29) instead of
// surfacing later as a broken login. Seed:SuperAdmin:Password comes from user-secrets / the environment, never from a
// committed file.
builder.Services.AddValidatedOptions<AuthOptions>(builder.Configuration, AuthOptions.SectionName);
builder.Services.AddValidatedOptions<SuperAdminSeedOptions>(builder.Configuration, SuperAdminSeedOptions.SectionName);
builder.Services.AddValidatedOptions<SystemAccountOptions>(builder.Configuration, SystemAccountOptions.SectionName);
builder.Services.AddValidatedOptions<RegistrationOptions>(builder.Configuration, RegistrationOptions.SectionName);

// R23: no trusted proxy is configured by default, so forwarded headers are ignored and every log row gets the socket
// address. Both are validated too, so a typo cannot silently disable the protection.
builder.Services.AddValidatedOptions<ReverseProxyOptions>(builder.Configuration, ReverseProxyOptions.SectionName);

// P16: the host migrates itself before serving requests (owner decision: Development and production both; set the flag
// to false to keep the step with a person).
builder.Services.AddValidatedOptions<DatabaseMigrationOptions>(builder.Configuration, DatabaseMigrationOptions.SectionName);

// Spec core: the notification outbox. Email:Host empty is valid (logging sender); once a host is set it is validated too.
builder.Services.AddValidatedOptions<NotificationOptions>(builder.Configuration, NotificationOptions.SectionName);
builder.Services.AddValidatedOptions<EmailOptions>(builder.Configuration, EmailOptions.SectionName);
builder.Services.AddValidatedOptions<MasterLayoutOptions>(builder.Configuration, MasterLayoutOptions.SectionName);
builder.Services.AddSingleton(serviceProvider => serviceProvider.GetRequiredService<IOptions<NotificationOptions>>().Value
    .ToNotificationSettings(serviceProvider.GetRequiredService<IOptions<MasterLayoutOptions>>().Value));

// The one place configuration becomes what the Application layer sees; the use cases never read IConfiguration.
builder.Services.AddSingleton(serviceProvider =>
    serviceProvider.GetRequiredService<IOptions<AuthOptions>>().Value.ToAuthSettings());

builder.Services.AddSingleton(serviceProvider =>
    serviceProvider.GetRequiredService<IOptions<SystemAccountOptions>>().Value.ToSettings());

builder.Services.AddSingleton(serviceProvider =>
    new RegistrationSettings(serviceProvider.GetRequiredService<IOptions<RegistrationOptions>>().Value.Mode));

// R49: finance puts this policy on their money-moving endpoints; it refuses the system account when configured to.
builder.Services.AddSingleton<IAuthorizationHandler, FinancialActionHandler>();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthPolicies.FinancialAction, policy => policy.AddRequirements(new FinancialActionRequirement()));

// Host-level services marked [AutoRegister] (cookie writer, problem factory, forced-password filter).
builder.Services.AddAutoRegisteredServices(Assembly.GetExecutingAssembly());
builder.Services.AddMemoryCache();                                    // short-lived session state, see ISessionGuard
builder.Services.AddAuthRateLimiting();                              // R10, per client IP
builder.Services.AddSessionTokenValidation();                         // R31 groundwork: a revoked session fails at once
builder.Services.AddAccessCookieAuthentication();                    // D2: the browser is signed in by the access cookie
builder.Services.Configure<MvcOptions>(options => options.Filters.Add<MustChangePasswordFilter>());  // R12
builder.Services.Configure<MvcOptions>(options =>
    options.ModelBinderProviders.Insert(0, new UpperSnakeEnumModelBinderProvider()));             // enums in ?role= / ?status= are UPPER_SNAKE too

// P17: schema first, data second - the runner must start before the seeders, which need the tables.
builder.Services.AddHostedService<DatabaseMigrationHostedService>();
builder.Services.AddHostedService<IamSeedHostedService>();             // R2: seeds the first Super Admin before requests are served
builder.Services.AddHostedService<NotificationTemplateSeedHostedService>();   // core N13: default templates, only the missing ones
builder.Services.AddHostedService<NotificationWorker>();                       // core N5: sends recorded notifications
builder.Services.AddHostedService<ExpiredSessionSweeper>();           // R24: closes sessions that simply ran out
builder.Services.AddHostedService<SuspensionLiftSweeper>();           // R32: lifts a suspension whose end date has passed

var app = builder.Build();

// R16: a work factor below the default is meant for tests and scripted checks only, so say so at every start.
var passwordOptions = app.Services.GetRequiredService<IOptions<AuthOptions>>().Value.Password;
if (passwordOptions.Iterations < PasswordOptions.DefaultIterations)
    app.Logger.LogWarning("Auth:Password:Iterations is {Iterations}, below the default {Default}: new password hashes are weaker. Use this only for tests.", passwordOptions.Iterations, PasswordOptions.DefaultIterations);

app.UseTrustedForwardedHeaders(app.Services.GetRequiredService<IOptions<ReverseProxyOptions>>().Value);
app.UseRequestLogging();
if (!app.Environment.IsDevelopment())
    app.UseHsts();                                  // P13: browsers stay on HTTPS once they have seen it (never on localhost)
app.UseHttpsRedirection();
app.UseSecurityHeaders();                           // P13: nosniff, no framing, referrer policy, CSP on HTML
app.UseSpaStaticFiles();                            // P13: the three Angular apps from wwwroot (only when it exists)
app.UseApiBuildingBlocks(builder.Configuration);   // exception handler, CORS, authentication, authorization
app.UseRateLimiter();                               // R10, after routing
app.MapApiBuildingBlocks();                        // controllers, /health, /openapi/v1.json (Development)
app.MapSpaFallback();                               // P13: deep links of the three apps reload; lowest priority, never for /api

app.Run();

public partial class Program;
