using Acme.Platform.Domain.Aggregates.User;
using Acme.Platform.Contracts;

namespace Acme.Platform.Application.Commands.DeactivateUser;

/// <summary>
/// Revokes a user's access without deleting them. Tenant scoping is ambient,
/// so the command names only the user it acts on.
/// </summary>
public sealed record DeactivateUserCommand(UserId UserId);
