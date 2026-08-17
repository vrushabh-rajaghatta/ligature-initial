namespace Acme.Api.Authentication;

/// <summary>
/// Named authorization policies (ADR-033). Endpoint gating happens through
/// these, never through role checks inside handlers — a handler that runs is
/// already authorized, exactly as a claim that is present is already verified.
/// </summary>
/// <remarks>
/// One policy, since ADR-066 left two roles. <c>PlatformAdministrator</c> is
/// gone with the tenant concept: it named someone who operated Acme across
/// tenants, and a deployment serves one customer.
/// <para>
/// Still exact-match, not hierarchical. Roles beyond these two arrive when a
/// feature needs them, not before.
/// </para>
/// </remarks>
public static class AcmePolicies
{
    public const string Administrator = "Administrator";
}
