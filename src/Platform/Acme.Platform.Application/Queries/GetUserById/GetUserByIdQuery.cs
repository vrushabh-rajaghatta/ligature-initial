using Acme.Platform.Domain.Aggregates.User;
using Acme.Platform.Contracts;

namespace Acme.Platform.Application.Queries.GetUserById;

/// <summary>
/// Reads a single user within the caller's tenant. A user belonging to another
/// tenant is reported as not found, never as forbidden.
/// </summary>
public sealed record GetUserByIdQuery(UserId UserId);
