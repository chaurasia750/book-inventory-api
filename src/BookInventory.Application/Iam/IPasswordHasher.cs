namespace BookInventory.Application.Iam;

/// <summary>
/// Hashes and verifies passwords (R16). A port, not a static helper, so the domain and the use cases never depend on a
/// hashing library and the implementation can be replaced without touching a single caller. Passwords are never logged
/// and never returned in a DTO.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Produces a self-describing hash: algorithm, cost and salt are stored inside it, so parameters can be
    /// raised later without invalidating existing hashes.</summary>
    string Hash(string password);

    /// <summary>Checks a password against a stored hash in constant time.</summary>
    PasswordVerificationResult Verify(string password, string passwordHash);

    /// <summary>
    /// R8: does the same amount of work as <see cref="Verify"/> without any account behind it, so an unknown identifier
    /// costs the caller as much time as a wrong password and the response time reveals nothing.
    /// </summary>
    void SpendVerificationTime(string password);
}

public enum PasswordVerificationResult
{
    Failed,

    Success,

    /// <summary>The password is correct but was hashed with weaker parameters than the current policy, so the caller
    /// should re-hash it on the next successful login.</summary>
    SuccessRehashNeeded
}
