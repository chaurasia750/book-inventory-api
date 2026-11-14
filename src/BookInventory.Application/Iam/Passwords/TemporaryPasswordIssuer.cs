using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Iam.Passwords;

/// <summary>R57: a freshly generated temporary password and its hash. The plain text goes into the answer once and is never stored.</summary>
public sealed record IssuedPassword(string Plain, string Hash);

/// <summary>Generates a temporary password (R57) and hashes it, so the use cases that hand one out do not each repeat the pair of calls.</summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class TemporaryPasswordIssuer(TemporaryPasswordGenerator generator, IPasswordHasher hasher)
{
    public IssuedPassword Issue()
    {
        var plain = generator.Generate();
        return new IssuedPassword(plain, hasher.Hash(plain));
    }
}
