namespace Acme.Platform.Domain.Aggregates.User;

/// <summary>
/// What a user is allowed to administer (ADR-033, narrowed by ADR-066).
/// Deliberately two values and not a permission system: the pressure on
/// authorization models is always to grow matrices before any feature needs
/// them.
/// </summary>
/// <remarks>
/// <c>PlatformAdministrator</c> was removed by ADR-066. It named a person who
/// operated Acme <em>across</em> tenants, and a deployment now serves exactly
/// one customer — there is no across. <c>TenantAdministrator</c> became
/// <c>Administrator</c> in the same change.
/// <para>
/// <b>The numbers are deliberately not renumbered.</b> The role persists as an
/// <c>int</c> (<c>UserConfiguration</c>), so reusing 1 for <c>Administrator</c>
/// would silently reinterpret every stored row. The gap is cheaper than a data
/// migration for a rename.
/// </para>
/// </remarks>
public enum UserRole
{
    /// <summary>
    /// Administers this deployment: invites and manages its users. The first
    /// user of a deployment, created by provisioning (ADR-066 decision 5).
    /// </summary>
    Administrator = 2,

    /// <summary>Does the work. The default for invited users.</summary>
    Member = 3
}
