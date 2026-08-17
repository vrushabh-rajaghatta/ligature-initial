using FluentAssertions;
using Microsoft.Extensions.Options;

using Acme.Platform.Application.Invitations;
using Acme.Platform.Infrastructure.Authentication;
using Acme.Platform.Application.Commands.InviteUser;
using Acme.Platform.Application.Tests.Fakes;
using Acme.Platform.Domain.Aggregates.User;
using Acme.Platform.Domain.ValueObjects;
using Acme.SharedKernel.Exceptions;

using UserAggregate = Acme.Platform.Domain.Aggregates.User.User;

namespace Acme.Platform.Application.Tests.Commands.InviteUser;

public sealed class InviteUserHandlerTests
{

    private static InviteUserCommand ValidCommand() =>
        new("John", "Doe", "john.doe@example.com");

    /// <summary>
    /// The real token issuer, not a fake: it does no I/O, and a fake would only
    /// prove that the fake was called.
    /// </summary>
    private static InvitationIssuer NewInvitationIssuer(
        FakeInvitationNotifier? notifier = null,
        FakeInvitationRepository? invitations = null) =>
        new(notifier ?? new FakeInvitationNotifier(),
            new InvitationTokenIssuer(
                new SecretTokenFactory(),
                Options.Create(new InvitationOptions { Days = 7 })),
            invitations ?? new FakeInvitationRepository());

    [Fact]
    public async Task Invite_Succeeds_ReturnsInvitedStatus()
    {
        var handler = new InviteUserHandler(
            NewInvitationIssuer(), new FakeUserPolicy(), new FakeUserRepository());

        var result = await handler.HandleAsync(ValidCommand(), CancellationToken.None);

        result.Status.Should().Be(UserStatus.Invited);
        result.Id.Should().NotBeNull();
    }

    [Fact]
    public async Task Invite_Succeeds_PersistsUserViaRepository()
    {
        var repository = new FakeUserRepository();
        var command = ValidCommand();
        var handler = new InviteUserHandler(
            NewInvitationIssuer(), new FakeUserPolicy(), repository);

        await handler.HandleAsync(command, CancellationToken.None);

        repository.Added.Should().NotBeNull();
        repository.Added!.Email.Value.Should().Be("john.doe@example.com");
        repository.Added.Status.Should().Be(UserStatus.Invited);
    }

    [Fact]
    public async Task Invite_WhenEmailNotUnique_Throws_AndDoesNotPersist()
    {
        var repository = new FakeUserRepository();
        var policy = new FakeUserPolicy(
            emailError: new BusinessRuleViolationException(PlatformErrors.EmailAlreadyInUse));
        var handler = new InviteUserHandler(
            NewInvitationIssuer(), policy, repository);

        var act = () => handler.HandleAsync(ValidCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleViolationException>();
        repository.Added.Should().BeNull();
    }

    [Fact]
    public async Task Invite_WhenEmailMalformed_ThrowsDomainException()
    {
        var handler = new InviteUserHandler(
            NewInvitationIssuer(), new FakeUserPolicy(), new FakeUserRepository());
        var command = new InviteUserCommand("John", "Doe", "not-an-email");

        var act = () => handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }
}
