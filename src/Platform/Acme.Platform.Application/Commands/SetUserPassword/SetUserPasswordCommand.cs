using Acme.Platform.Domain.Aggregates.User;
using Acme.Platform.Contracts;

namespace Acme.Platform.Application.Commands.SetUserPassword;

public sealed record SetUserPasswordCommand(UserId UserId, string? Password);
