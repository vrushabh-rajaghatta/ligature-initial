using Acme.Platform.Domain.Aggregates.User;
using Acme.Platform.Contracts;

namespace Acme.Platform.Application.Commands.InviteUser;

public sealed record InviteUserResult(
    UserId Id,
    UserStatus Status);
