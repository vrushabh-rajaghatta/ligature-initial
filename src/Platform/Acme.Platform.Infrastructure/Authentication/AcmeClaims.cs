namespace Acme.Platform.Infrastructure.Authentication;

/// <summary>
/// Claim names Acme puts in its own tokens. Kept in one place because the
/// issuer writes them and the authentication middleware will read them, and a
/// typo in either would be a silent authorization failure.
/// </summary>
public static class AcmeClaims
{
    /// <summary>
    /// The user's role, as the enum member's name. The issuer once refused
    /// role claims on principle ("identity, not an authorization snapshot");
    /// ADR-033 reverses that deliberately: staleness is capped at the
    /// fifteen-minute token lifetime, a demotion can end sessions through the
    /// ADR-028 machinery, and the alternative was a database read on every
    /// authorization check.
    /// </summary>
    public const string Role = "acme:role";
}
