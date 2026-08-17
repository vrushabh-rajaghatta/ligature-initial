using Acme.Platform.Application.Common;
using Acme.Platform.Application.Services;
using Acme.Platform.Domain.Aggregates.User;
using Acme.Platform.Domain.ValueObjects;
using Acme.Platform.Contracts;

namespace Acme.Platform.Application.Commands.UpdateUserProfile;

public sealed class UpdateUserProfileHandler
{
    private readonly IUserPolicy _userPolicy;
    private readonly IUserRepository _repository;

    public UpdateUserProfileHandler(
        IUserPolicy userPolicy,
        IUserRepository repository)
    {
        _userPolicy = userPolicy;
        _repository = repository;
    }

    public async Task HandleAsync(
        UpdateUserProfileCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _repository.GetRequiredAsync(
            command.UserId, cancellationToken);

        var email = Email.Create(command.Email);

        // Excluding the user itself so
        // an unchanged email never collides with its own row.
        await _userPolicy.EnsureEmailIsUniqueForUpdateAsync(
            user.Id,
            email,
            cancellationToken);

        // The aggregate owns the invariants (names required, email valid) and
        // the no-op semantics; the handler never reimplements them.
        user.ChangeName(command.FirstName, command.LastName);
        user.ChangeEmail(email);

        await _repository.UpdateAsync(user, cancellationToken);
    }
}
