using System.Net;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BookInventory.BuildingBlocks.Configuration.Exceptions
{
    /// <summary>
    /// Turns any unhandled exception into an RFC 9457 <c>application/problem+json</c> response with a
    /// <c>traceId</c>. Business failures (<see cref="AppException"/>) map to 4xx and expose their message
    /// <c>code</c> plus the catalogue text; everything unexpected is a generic 500 - the exception text and stack trace
    /// are only added in Development.
    /// </summary>
    public sealed class GlobalExceptionHandler(
        IProblemDetailsService problemDetails,
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment,
        IMessageCatalog catalog) : IExceptionHandler
    {
        private const int ClientClosedRequest = 499;
        private const string GenericDetail = "The request could not be processed.";

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (status, detail, code) = Map(exception);

            if (status == ClientClosedRequest)
                logger.LogDebug("Request cancelled by the client.");
            else if (status >= StatusCodes.Status500InternalServerError)
                logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
            else
                logger.LogWarning(exception, "Request failed with {StatusCode}: {Message}", status, exception.Message);

            httpContext.Response.StatusCode = status;

            var problem = new ProblemDetails
            {
                Status = status,
                Title = status == ClientClosedRequest ? "Request cancelled." : ReasonPhrases.GetReasonPhrase(status),
                Detail = detail
            };
            if (code is not null)
                problem.Extensions["code"] = code;
            if (exception is AppException { Subject: { } subject })
                problem.Extensions["subject"] = subject;
            if (environment.IsDevelopment() && status >= StatusCodes.Status500InternalServerError)
                problem.Extensions["exception"] = exception.ToString();

            return await problemDetails.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problem
            });
        }

        private (int Status, string? Detail, string? Code) Map(Exception exception) => exception switch
        {
            AppException app => (StatusOf(app), catalog.Find(app.Code) ?? GenericDetail, app.Code),
            OperationCanceledException => (ClientClosedRequest, null, null),
            InvalidDataException => ((int)HttpStatusCode.BadRequest, Message(exception), null),
            KeyNotFoundException => ((int)HttpStatusCode.NotFound, Message(exception), null),
            MemberAccessException => ((int)HttpStatusCode.Unauthorized, Message(exception), null),
            UnauthorizedAccessException => ((int)HttpStatusCode.Forbidden, Message(exception), null),
            DbUpdateConcurrencyException => ((int)HttpStatusCode.Conflict,
                "The record was modified by someone else. Reload and try again.", null),
            DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } } =>
                ((int)HttpStatusCode.Conflict, "A record with the same value already exists.", null),
            DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation } } =>
                ((int)HttpStatusCode.Conflict,
                    "The record is referenced by, or refers to, another record that does not allow this change.", null),
            _ => ((int)HttpStatusCode.InternalServerError, null, null)
        };

        private static int StatusOf(AppException exception) => exception switch
        {
            ValidationFailureException => (int)HttpStatusCode.BadRequest,
            NotFoundException => (int)HttpStatusCode.NotFound,
            ConflictException => (int)HttpStatusCode.Conflict,
            ForbiddenException => (int)HttpStatusCode.Forbidden,
            UnauthenticatedException => (int)HttpStatusCode.Unauthorized,
            ServerFailureException => (int)HttpStatusCode.InternalServerError,
            _ => (int)HttpStatusCode.BadRequest
        };

        private static string Message(Exception exception) => exception.InnerException?.Message ?? exception.Message;
    }
}
