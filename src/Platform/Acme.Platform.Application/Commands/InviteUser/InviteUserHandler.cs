using Acme.Platform.Application.Invitations;
using Acme.Platform.Application.Services;
using Acme.Platform.Domain.Aggregates.User;
using Acme.Platform.Domain.ValueObjects;
using Acme.SharedKernel.Abstractions;

using UserAggregate = Acme.Platform.Domain.Aggregates.User.User;

namespace Acme.Platform.Application.Commands.InviteUser;

public sealed class InviteUserHandler
{
    private readonly InvitationIssuer _invitations;
    private readonly IUserPolicy _userPolicy;
    private readonly IUserRepository _repository;
    private readonly ITenantContext _tenantContext;

    public InviteUserHandler(
        InvitationIssuer invitations,
        IUserPolicy userPolicy,
        IUserRepository repository,
        ITenantContext tenantContext)
    {
        _invitations = invitations;
        _userPolicy = userPolicy;
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<InviteUserResult> HandleAsync(
        InviteUserCommand command,
        CancellationToken cancellationToken)
    {
        // You invite people into your own tenant. The caller no longer
        // chooses which one, so inviting into someone else's tenant is not an
        // authorization check that could be forgotten - it is unexpressible.
        var tenantId = _tenantContext.TenantId;

        await _userPolicy.EnsureTenantCanAcceptUsersAsync(
            tenantId,
            cancellationToken);

        var email = Email.Create(command.Email);

        // Unscoped by tenant: an email identifies exactly one user across
        // Acme, so an address already invited elsewhere is a conflict here too
        // (ADR-021).
        await _userPolicy.EnsureEmailIsUniqueAsync(
            email,
            cancellationToken);

        var user = UserAggregate.CreateForTenant(
            tenantId,
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
