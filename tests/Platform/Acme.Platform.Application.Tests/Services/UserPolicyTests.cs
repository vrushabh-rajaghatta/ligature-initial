using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using Acme.Persistence;
using Acme.Platform.Domain.ValueObjects;
using Acme.Platform.Infrastructure.Services;

using UserAggregate = Acme.Platform.Domain.Aggregates.User.User;
using Acme.SharedKernel.Exceptions;

namespace Acme.Platform.Application.Tests.Services;

// Integration tests for the uniqueness rules — the exclude-self behaviour is
// SQL, so fakes cannot prove it. Scoped to a throwaway TenantId.
[Collection(PlatformDatabase.Collection)]
public sealed class UserPolicyTests : IAsyncLifetime
{
    private readonly PlatformDatabase _database;

    public UserPolicyTests(PlatformDatabase database)
    {
        _database = database;
    }

    private UserAggregate _existing = default!;
    private UserAggregate _other = default!;
    private UserAggregate _elsewhere = default!;

    private AcmeDbContext NewContext() =>
        new(_database.Options);

    public async Task InitializeAsync()
    {
        await using var context = NewContext();

        _existing = UserAggregate.Create(Email.Create("taken@policy.example"), "Taken", "User");

        _other = UserAggregate.Create(Email.Create("other@policy.example"), "Other", "User");

        _elsewhere = UserAggregate.Create(Email.Create("elsewhere@policy.example"),
            "Else",
            "Where");

        context.Users.AddRange(_existing, _other, _elsewhere);

        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await using var context = NewContext();

        await context.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"Users\" WHERE \"Email\" LIKE '%@policy.example'");
    }

    [Fact]
    public async Task Update_allows_a_user_to_keep_its_own_email()
    {
        await using var context = NewContext();
        var policy = new UserPolicy(context);

        var act = () => policy.EnsureEmailIsUniqueForUpdateAsync(
            _existing.Id,
            Email.Create("taken@policy.example"),
            CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Update_rejects_an_email_owned_by_another_user()
    {
        await using var context = NewContext();
        var policy = new UserPolicy(context);

        var act = () => policy.EnsureEmailIsUniqueForUpdateAsync(
            _other.Id,
            Email.Create("taken@policy.example"),
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleViolationException>();
    }

    [Fact]
    public async Task Invite_rejects_an_email_already_used_in_another_organization()
    {
        // The rule this ADR-021 slice exists for. Before the change this
        // passed - uniqueness was scoped to the organization, so the same
        // address could be invited twice and login could not resolve a user.
        await using var context = NewContext();
        var policy = new UserPolicy(context);

        var act = () => policy.EnsureEmailIsUniqueAsync(
            Email.Create("taken@policy.example"),
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleViolationException>();
    }

    [Fact]
    public async Task Update_rejects_an_email_owned_by_a_user_in_another_organization()
    {
        await using var context = NewContext();
        var policy = new UserPolicy(context);

        var act = () => policy.EnsureEmailIsUniqueForUpdateAsync(
            _elsewhere.Id,
            Email.Create("taken@policy.example"),
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleViolationException>();
    }

    [Fact]
    public async Task Update_allows_a_genuinely_new_email()
    {
        await using var context = NewContext();
        var policy = new UserPolicy(context);

        var act = () => policy.EnsureEmailIsUniqueForUpdateAsync(
            _existing.Id,
            Email.Create("brand.new@policy.example"),
            CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
