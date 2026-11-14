using System.Security.Cryptography;
using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Authentication;

namespace BookInventory.Application.Iam.Passwords;

/// <summary>
/// R57: the random temporary password a Super Admin sees once when creating a staff account (Plan 0006 uses it). It comes
/// from the cryptographic random generator, always meets the configured policy (R16) - at least the required number of
/// upper case letters, lower case letters and digits, and the minimum length - and leaves out characters that are easy to
/// misread when it is read aloud or copied by hand (<c>0 O 1 l I</c>).
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class TemporaryPasswordGenerator(AuthSettings settings)
{
    private const int MinimumGeneratedLength = 14;
    private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Lower = "abcdefghijkmnopqrstuvwxyz";
    private const string Digits = "23456789";

    public string Generate()
    {
        var policy = settings.Password;
        var length = Math.Max(MinimumGeneratedLength, policy.MinimumLength);

        var characters = new List<char>(length);
        characters.AddRange(Pick(Upper, policy.RequiredUpperCase));
        characters.AddRange(Pick(Lower, policy.RequiredLowerCase));
        characters.AddRange(Pick(Digits, policy.RequiredDigits));

        var all = Upper + Lower + Digits;
        while (characters.Count < length)
            characters.Add(all[RandomNumberGenerator.GetInt32(all.Length)]);

        RandomNumberGenerator.Shuffle(CollectionsSpan(characters));
        return new string(characters.ToArray());
    }

    private static IEnumerable<char> Pick(string alphabet, int count) =>
        Enumerable.Range(0, count).Select(_ => alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)]);

    private static Span<char> CollectionsSpan(List<char> list) => System.Runtime.InteropServices.CollectionsMarshal.AsSpan(list);
}
