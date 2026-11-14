using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.Options;
using BookInventory.Application.Iam;
using BookInventory.Infrastructure.Iam.Configuration;

namespace BookInventory.Infrastructure.Iam;

/// <summary>
/// R16 hashing, built on the BCL only (<see cref="Rfc2898DeriveBytes"/> with HMAC-SHA512), so the project takes no
/// extra package. The stored format is self-describing:
/// <c>$pbkdf2-sha512$&lt;iterations&gt;$&lt;base64 salt&gt;$&lt;base64 hash&gt;</c> - which means the cost can be raised
/// later and old hashes still verify (answering <see cref="PasswordVerificationResult.SuccessRehashNeeded"/> so the
/// caller re-hashes them on the next login) instead of invalidating every password.
/// <para>
/// Iteration count and salt size follow the OWASP guidance for PBKDF2-HMAC-SHA512. The comparison is fixed-time, and
/// nothing about the password is ever logged.
/// </para>
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IPasswordHasher))]
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const string Algorithm = "pbkdf2-sha512";
    private const int SaltSize = 16;
    private const int HashSize = 64;

    private readonly int _iterations;

    /// <summary>The work factor of new hashes comes from <c>Auth:Password:Iterations</c> (R16, R54).</summary>
    public Pbkdf2PasswordHasher(IOptions<AuthOptions> options) : this(options.Value.Password.Iterations) { }

    internal Pbkdf2PasswordHasher(int iterations) => _iterations = iterations;

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Derive(password, salt, _iterations);

        // Written out by hand rather than with string.Join, because the value starts with the separator:
        // "$pbkdf2-sha512$<iterations>$<salt>$<hash>", which Verify splits into exactly five parts.
        return $"${Algorithm}${_iterations.ToString(CultureInfo.InvariantCulture)}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public PasswordVerificationResult Verify(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
            return PasswordVerificationResult.Failed;

        var parts = passwordHash.Split('$');
        // "", "pbkdf2-sha512", iterations, salt, hash
        if (parts.Length != 5
            || parts[0].Length != 0
            || !string.Equals(parts[1], Algorithm, StringComparison.Ordinal)
            || !int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out var iterations)
            || iterations < 1)
        {
            return PasswordVerificationResult.Failed;
        }

        byte[] salt;
        byte[] expected;
        try
        {
            salt = Convert.FromBase64String(parts[3]);
            expected = Convert.FromBase64String(parts[4]);
        }
        catch (FormatException)
        {
            return PasswordVerificationResult.Failed;
        }

        if (salt.Length == 0 || expected.Length == 0)
            return PasswordVerificationResult.Failed;

        var actual = Derive(password, salt, iterations);
        if (!CryptographicOperations.FixedTimeEquals(actual, expected))
            return PasswordVerificationResult.Failed;

        return iterations < _iterations
            ? PasswordVerificationResult.SuccessRehashNeeded
            : PasswordVerificationResult.Success;
    }

    /// <summary>A fixed salt and the current cost: the same work as a real verification, against nothing.</summary>
    public void SpendVerificationTime(string password) =>
        Derive(password, new byte[SaltSize], _iterations);

    private static byte[] Derive(string password, byte[] salt, int iterations) =>
        Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, HashAlgorithmName.SHA512, HashSize);
}
