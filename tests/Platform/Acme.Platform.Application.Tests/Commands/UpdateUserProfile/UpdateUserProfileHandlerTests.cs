using FluentAssertions;

using Acme.SharedKernel.Primitives;
using Acme.Platform.Application.Commands.UpdateUserProfile;
using Acme.Platform.Application.Tests.Fakes;
using Acme.Platform.Domain.Aggregates.User;
using Acme.Platform.Domain.ValueObjects;
using Acme.SharedKernel.Exceptions;

using UserAggregate = Acme.Platform.Domain.Aggregates.User.User;
using Acme.Platform.Contracts;

namespace Acme.Platform.Application.Tests.Commands.UpdateUserProfile;

public sealed class UpdateUserProfileHandlerTests
{
    private static readonly TenantId Organization = TenantId.New();

    private static UserAggregate ExistingUser() =>
        UserAggregate.CreateForTenant(
            Organization,
            Email.Create("john.doe@example.com"),
            "John",
            "Doe");

    private static UpdateUserProfileCommand Command(UserId userId) =>
        new(userId, "Jane", "Smith", "jane.smith@example.com");

    [Fact]
    public async Task Updates_the_profile_and_persists_it()
    {
        var user = ExistingUser();
        var repository = new FakeUserRepository(user);
        var handler = new UpdateUserProfileHandler(
            new FakeUserPolicy(), repository, new FakeTenantContext(Organization));

        await handler.HandleAsync(Command(user.Id), CancellationToken.None);

        repository.Updated.Should().NotBeNull();
        repository.Updated!.FirstName.Should().Be("Jane");
        repository.Updated.LastName.Should().Be("Smith");
        repository.Updated.Email.Value.Should().Be("jane.smith@example.com");
    }

    [Fact]
    public async Task Leaves_status_and_organization_untouched()
    {
        var user = ExistingUser();
        var repository = new FakeUserRepository(user);
        var handler = new UpdateUserProfileHandler(
            new FakeUserPolicy(), repository, new FakeTenantContext(Organization));

        await handler.HandleAsync(Command(user.Id), CancellationToken.None);

        repository.Updated!.Status.Should().Be(UserStatus.Invited);
        repository.Updated.TenantId.Should().Be(Organization);
    }

    [Fact]
    public async Task Throws_not_found_when_the_user_does_not_exist()
    {
        var repository = new FakeUserRepository();
        var handler = new UpdateUserProfileHandler(
            new FakeUserPolicy(), repository, new FakeTenantContext(Organization));

        var act = () => handler.HandleAsync(
            Command(UserId.New()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        repository.Updated.Should().BeNull();
    }

    [Fact]
    public async Task Throws_not_found_when_the_user_belongs_to_another_organization()
    {
        var user = ExistingUser();
        var repository = new FakeUserRepository(user);
        // The caller's tenant is a different organization, so the user is
        // invisible. The command can no longer carry a tenant at all.
        var handler = new UpdateUserProfileHandler(
            new FakeUserPolicy(), repository,
            new FakeTenantContext(TenantId.New()));

        var act = () => handler.HandleAsync(
            Command(user.Id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        repository.Updated.Should().BeNull();
    }

    [Fact]
    public async Task Throws_when_the_email_is_already_used_by_someone_else()
    {
        var user = ExistingUser();
        var repository = new FakeUserRepository(user);
        var policy = new FakeUserPolicy(
            updateEmailError: new BusinessRuleViolationException(
                PlatformErrors.EmailAlreadyInUse));
        var handler = new UpdateUserProfileHandler(
            policy, repository, new FakeTenantContext(Organization));

        var act = () => handler.HandleAsync(
            Command(user.Id), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleViolationException>();
        repository.Updated.Should().BeNull();
    }

    [Fact]
    public async Task Rejects_an_invalid_email_through_the_value_object()
    {
        var user = ExistingUser();
        var repository = new FakeUserRepository(user);
        var handler = new UpdateUserProfileHandler(
            new FakeUserPolicy(), repository, new FakeTenantContext(Organization));

        var command = Command(user.Id) with { Email = "not-an-email" };

        var act = () => handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
        repository.Updated.Should().BeNull();
    }

    [Fact]
    public async Task Rejects_an_empty_name_through_the_aggregate()
    {
        var user = ExistingUser();
        var repository = new FakeUserRepository(user);
        var handler = new UpdateUserProfileHandler(
            new FakeUserPolicy(), repository, new FakeTenantContext(Organization));

        var command = Command(user.Id) with { FirstName = "   " };

        var act = () => handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
        repository.Updated.Should().BeNull();
    }
}
