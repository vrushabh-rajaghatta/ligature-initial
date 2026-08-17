namespace Acme.Platform.Application;

/// <summary>
/// Business-rule violation messages surfaced by the platform policies.
/// </summary>
public static class PlatformErrors
{
    public const string UserNotFound =
        "User not found.";

    // An email identifies exactly one user in this deployment (ADR-021, as
    // narrowed by ADR-066). The wording states the rule without disclosing
    // anything about the colliding record.
    public const string EmailAlreadyInUse =
        "A user with this email address already exists.";
}
