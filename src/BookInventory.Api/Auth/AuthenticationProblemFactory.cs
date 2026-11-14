using System.Net;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Domain.Iam;

namespace BookInventory.Api.Auth;

/// <summary>
/// Turns a refused authentication attempt into the RFC 9457 answer the spec asks for: the status, the human text from
/// the message catalogue (R55) and the stable <c>code</c> the UI switches on. The global exception handler cannot do
/// this for login, because throwing would roll the failed attempt back (R21) - a refusal is a returned result.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class AuthenticationProblemFactory(IMessageCatalog catalog)
{
    private const string GenericDetail = "The request could not be processed.";

    public ProblemDetails For(AuthFailure failure) => Build(failure.Status, failure.Code);

    /// <summary>R12: 403 for every endpoint except change-password, me and logout.</summary>
    public ProblemDetails PasswordChangeRequired() => Build(HttpStatusCode.Forbidden, IamMessages.UserPasswordChangeRequired);

    /// <summary>R10: 429 with <c>Retry-After</c> when the per-IP limit is hit.</summary>
    public ProblemDetails RateLimited() => Build(HttpStatusCode.TooManyRequests, IamMessages.AuthRateLimited);

    private ProblemDetails Build(HttpStatusCode status, string code) =>
        new()
        {
            Status = (int)status,
            Title = ReasonPhrases.GetReasonPhrase((int)status),
            Detail = catalog.Find(code) ?? GenericDetail,
            Extensions = { ["code"] = code }
        };
}
