namespace BookInventory.BuildingBlocks.Domain.Exceptions
{
    /// <summary>
    /// An expected, business-level failure. Carries a stable message <see cref="Code"/>
    /// (for example <c>iam.user.invalid_transition</c>); the human text is looked up in the message catalogue by code,
    /// so no user-facing text is hard-coded. The API layer maps each subtype to an HTTP status.
    /// </summary>
    public abstract class AppException(string code, string? message = null, Exception? innerException = null)
        : Exception(message ?? code, innerException)
    {
        public string Code { get; } = code;

        /// <summary>What the failure is about, when the code alone does not say (for example the item code and class of an order line); returned as the problem details <c>subject</c>.</summary>
        public string? Subject { get; init; }
    }

    /// <summary>The request is invalid or breaks a business rule (HTTP 400).</summary>
    public class ValidationFailureException(string code, string? message = null, Exception? innerException = null)
        : AppException(code, message, innerException);

    /// <summary>The referenced record does not exist, or the caller may not know it exists (HTTP 404).</summary>
    public class NotFoundException(string code, string? message = null) : AppException(code, message);

    /// <summary>The request conflicts with the current state, e.g. a duplicate or an invalid transition (HTTP 409).</summary>
    public class ConflictException(string code, string? message = null) : AppException(code, message);

    /// <summary>The caller is authenticated but not allowed to do this (HTTP 403).</summary>
    public class ForbiddenException(string code, string? message = null) : AppException(code, message);

    /// <summary>The caller is not authenticated or the credentials/token are not valid (HTTP 401).</summary>
    public class UnauthenticatedException(string code, string? message = null) : AppException(code, message);

    /// <summary>The server could not complete the request through no fault of the caller, e.g. a document could not be rendered (HTTP 500).</summary>
    public class ServerFailureException(string code, string? message = null, Exception? innerException = null)
        : AppException(code, message, innerException);
}
