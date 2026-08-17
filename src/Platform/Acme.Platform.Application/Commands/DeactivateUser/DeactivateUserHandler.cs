using Acme.Platform.Application.Common;
using Acme.Platform.Domain.Aggregates.User;
using Acme.Platform.Contracts;

namespace Acme.Platform.Application.Commands.DeactivateUser;

/// <summary>
/// Load, invoke the aggregate behaviour, persist. Deactivation preserves the
/// profile: this is a revocation of access, not a deletion. The lifecycle rule
/// (including idempotency) belongs to the aggregate.
/// </summary>
public sealed class DeactivateUserHandler
{
    private readonly IUserRepository _repository;

    public DeactivateUserHandler(
        IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        DeactivateUserCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _repository.GetRequiredAsync(
            command.UserId, cancellationToken);

        user.Deactivate();

        await _repository.UpdateAsync(user, cancellationToken);
    }
}
