using Acme.Platform.Application.Common;
using Acme.Platform.Application.Invitations;
using Acme.Platform.Domain.Aggregates.User;
using Acme.SharedKernel.Exceptions;
using Acme.Platform.Contracts;

namespace Acme.Platform.Application.Commands.ResendInvitation;

/// <summary>
/// Issues a fresh invitation and retires the previous one.
/// </summary>
/// <remarks>
/// An administrator is asking, so the same rules as every other
/// user-administration command apply.
///
/// Also the remediation path for users invited before invitations carried
/// tokens — they have no invitation at all, and this gives them one.
/// </remarks>
public sealed class ResendInvitationHandler
{
    private readonly InvitationIssuer _invitations;
    private readonly IUserRepository _repository;

    public ResendInvitationHandler(
        InvitationIssuer invitations,
        IUserRepository repository)
    {
        _invitations = invitations;
        _repository = repository;
    }

    public async Task HandleAsync(
        ResendInvitationCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _repository.GetRequiredAsync(
            command.UserId, cancellationToken);

        // Only someone still waiting to accept can be re-invited. Resending to
        // an active user would hand out a token that acceptance would refuse,
        // and resending to a deactivated one would undo the deactivation's
        // intent.
        if (user.Status != UserStatus.Invited)
        {
            throw new BusinessRuleViolationException(
                UserErrors.OnlyInvitedUsersCanBeReinvited);
        }

        await _invitations.IssueAsync(user, DateTime.UtcNow, cancellationToken);
    }
}
