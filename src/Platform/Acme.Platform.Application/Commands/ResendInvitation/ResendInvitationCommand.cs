using Acme.Platform.Domain.Aggregates.User;
using Acme.Platform.Contracts;

namespace Acme.Platform.Application.Commands.ResendInvitation;

public sealed record ResendInvitationCommand(UserId UserId);
