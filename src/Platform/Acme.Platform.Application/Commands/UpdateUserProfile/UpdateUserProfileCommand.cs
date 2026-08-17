using Acme.Platform.Domain.Aggregates.User;
using Acme.Platform.Contracts;

namespace Acme.Platform.Application.Commands.UpdateUserProfile;

/// <summary>
/// Updates a user's profile - name and email only. Status, roles, permissions
/// and organization membership are separate capabilities. Tenant scoping is
/// ambient rather than a property of the command.
/// </summary>
public sealed record UpdateUserProfileCommand(
    UserId UserId,
    string FirstName,
    string LastName,
    string Email);
