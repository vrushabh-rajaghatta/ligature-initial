using Acme.Platform.Domain.Aggregates.User;

namespace Acme.Api.Endpoints.Platform;

public sealed record InviteUserResponse(
    Guid Id,
    UserStatus Status);
