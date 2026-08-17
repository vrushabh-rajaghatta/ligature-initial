using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using Acme.Persistence;
using Acme.Platform.Application.Queries.GetUserById;
using Acme.Platform.Domain.Aggregates.User;
using Acme.Platform.Domain.ValueObjects;

using UserAggregate = Acme.Platform.Domain.Aggregates.User.User;
using Acme.SharedKernel.Exceptions;
using Acme.Platform.Application.Tests.Fakes;
using Acme.Platform.Contracts;

namespace Acme.Platform.Application.Tests.Queries.GetUserById;

// Integration tests against the real dev Postgres, scoped to a throwaway
// TenantId so they cannot collide with existing data.
[Collection(PlatformDatabase.Collection)]
public sealed class GetUserByIdHandlerTests : IAsyncLifetime
{
    private readonly PlatformDatabase _database;

    public GetUserByIdHandlerTests(PlatformDatabase database)
    {
        _database = database;
    }


    private UserId _userId = default!;

    private DbContextOptions<AcmeDbContext> Options() =>
        _database.Options;

    // The context carries the same tenant the handler is scoped to, so the
    // global query filter (ADR-031) resolves to this test's rows.
    private AcmeDbContext NewContext() =>
        new(Options());

    public async Task InitializeAsync()
    {
        await using var context = NewContext();

        await UserTables.ClearAsync(context);

        var user = UserAggregate.Create(Email.Create("grace.hopper@details.example"),
            "Grace",
            "Hopper");

        _userId = user.Id;

        context.Users.Add(user);

        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await using var context = NewContext();

        await UserTables.ClearAsync(context);
    }

    private async Task<UserDetails> QueryAsync(
        GetUserByIdQuery query)
    {
        await using var context = NewContext();

        return await new GetUserByIdHandler(
                context)
            .HandleAsync(query, CancellationToken.None);
    }

    [Fact]
    public async Task Returns_the_user_when_found()
    {
        var user = await QueryAsync(
            new GetUserByIdQuery(_userId));

        user.Id.Should().Be(_userId.Value);
    }

    [Fact]
    public async Task Projects_every_field_correctly()
    {
        var user = await QueryAsync(
            new GetUserByIdQuery(_userId));

        user.FirstName.Should().Be("Grace");
        user.LastName.Should().Be("Hopper");
        user.Email.Should().Be("grace.hopper@details.example");
        user.Status.Should().Be(UserStatus.Invited);
        user.CreatedOn.Should().NotBe(default);
    }

    [Fact]
    public async Task Throws_not_found_when_the_user_does_not_exist()
    {
        var act = () => QueryAsync(
            new GetUserByIdQuery(UserId.New()));

        await act.Should().ThrowAsync<NotFoundException>();
    }

}
