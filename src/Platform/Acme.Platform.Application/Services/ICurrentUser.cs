using Acme.Platform.Domain.Aggregates.User;
using Acme.Platform.Domain.ValueObjects;
using Acme.Platform.Contracts;

namespace Acme.Platform.Application.Services;

/// <summary>
/// The authenticated caller behind the current request.
/// </summary>
/// <remarks>
/// <para>
/// Deliberately four members and no more. The pressure on a type like this is
/// always to grow — a display name here, the organization's name there,
/// because a page needs it — until every service depends on it and none of
/// them can say why. Everything absent from this interface can be resolved
/// from <see cref="UserId"/> by whoever actually needs it.
/// </para>
/// <para>
/// <see cref="Role"/> arrived with ADR-033, in the minimal shape
/// the epic needed — not the permission matrix this doc once warned about
/// guessing at. Permissions beyond the role stay absent until decided.
/// </para>
/// <para>
/// Had a <c>TenantId</c> sibling in <c>ITenantContext</c> until ADR-066. With
/// one deployment per customer there is no scoping question left to answer —
/// only <em>which person is calling</em>, which is a Platform concept and
/// typed accordingly.
/// </para>
/// </remarks>
public interface ICurrentUser
{
    /// <summary>
    /// Whether the request carried a valid token. This is the only member safe
    /// to read on an anonymous request; the others throw.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// The caller's user id. Throws when the request is not authenticated,
    /// rather than returning an empty id — an unauthenticated
    /// default must never be mistakable for a real caller.
    /// </summary>
    UserId UserId { get; }

    /// <summary>
    /// The caller's email address. Throws when unauthenticated.
    /// </summary>
    Email Email { get; }

    /// <summary>
    /// The caller's role, as their token states it (ADR-033). Throws when
    /// unauthenticated. Endpoint gating uses the authorization policies, not
    /// this — this exists for the rare handler and for /me.
    /// </summary>
    UserRole Role { get; }
}
