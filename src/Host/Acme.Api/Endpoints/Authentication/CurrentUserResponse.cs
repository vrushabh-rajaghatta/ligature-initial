namespace Acme.Api.Endpoints.Authentication;

/// <summary>
/// Exactly what the token carries, and nothing resolved from the database.
/// This endpoint answers "is my token still good, and who does it say I am",
/// so reading a user row to enrich it would answer a different question more
/// slowly.
/// </summary>
/// <remarks>
/// Carried a <c>TenantId</c> until ADR-066, mirroring the claim that used to
/// travel in the token. Neither exists now.
/// </remarks>
public sealed record CurrentUserResponse(
    Guid UserId,
    string Email,
    string Role);
