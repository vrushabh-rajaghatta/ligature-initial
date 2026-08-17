using Acme.Platform.Application.Invitations;
using Acme.Platform.Application.Services;
using Acme.Platform.Domain.Aggregates.User;
using Acme.Platform.Domain.ValueObjects;

using UserAggregate = Acme.Platform.Domain.Aggregates.User.User;

namespace Acme.Platform.Application.Commands.InviteUser;

public sealed class InviteUserHandler
{
    private readonly InvitationIssuer _invitations;
    private readonly IUserPolicy _userPolicy;
    private readonly IUserRepository _repository;

    public InviteUserHandler(
        InvitationIssuer invitations,
        IUserPolicy userPolicy,
        IUserRepository repository)
    {
        _invitations = invitations;
        _userPolicy = userPolicy;
        _repository = repository;
    }

    public async Task<InviteUserResult> HandleAsync(
        InviteUserCommand command,
        CancellationToken cancellationToken)
    {
        var email = Email.Create(command.Email);

        // An email identifies exactly one user in this deployment, so an
        // address already invited is a conflict (ADR-021, as narrowed by
        // ADR-066).
        await _userPolicy.EnsureEmailIsUniqueAsync(
            email,
            cancellationToken);

        var user = UserAggregate.Create(
            email,
            command.FirstName,
            command.LastName);

        await _repository.AddAsync(user, cancellationToken);

        // The user row alone cannot be accepted. An invited user without an
        // invitation is a person who can never sign in, so the two are created
        // together (ADR-027).
        await _invitations.IssueAsync(user, DateTime.UtcNow, cancellationToken);

        return new InviteUserResult(user.Id, user.Status);
    }
}
