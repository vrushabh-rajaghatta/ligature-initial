using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using Npgsql;

namespace Acme.DocumentManagement.Persistence.Tests;

/// <summary>
/// <b>Green must mean "the schema is current", not "nothing collided".</b>
/// </summary>
/// <remarks>
/// A stale schema only turns a test red when a migration happens to touch a
/// read path some test already exercises — so a suite can run several
/// migrations behind and stay green until the day everything fails at once.
/// This asserts the thing that is otherwise assumed. The guarantee itself is
/// structural, enforced by <c>AcmeTestDatabase</c>; what these tests add is a
/// <em>readable</em> statement of it, which a person can run and believe
/// (ADR-064).
/// </remarks>
[Collection(DocumentManagementDatabase.Collection)]
public sealed class SchemaCurrencyTests
{
    private readonly DocumentManagementDatabase _database;

    public SchemaCurrencyTests(DocumentManagementDatabase database)
    {
        _database = database;
    }

    [Fact]
    public async Task The_schema_this_suite_runs_against_holds_every_migration()
    {
        await using var context = _database.NewContext();

        var pending = await context.Database.GetPendingMigrationsAsync();

        pending.Should().BeEmpty(
            "the suite provisions its own database from the migration chain, so "
            + "there is no window in which a schema can be behind it (ADR-064)");

        _database.AppliedMigrations.Should().BeEquivalentTo(
            context.Database.GetMigrations(),
            "every migration in source control should be recorded as applied — "
            + "a shorter list means the schema came from somewhere other than "
            + "the chain");
    }

    /// <summary>
    /// <b>And it is nobody's working database.</b> The assertion above would
    /// also pass against a hand-maintained database that somebody had just
    /// migrated — the state that keeps quietly reverting.
    /// </summary>
    [Fact]
    public void This_is_not_a_database_anyone_maintains()
    {
        _database.Name.Should().StartWith("acme_test_",
            "a provisioned database is created for this run and dropped after "
            + "it; if the suite is pointed at a durable one, every guarantee "
            + "here is back to depending on somebody having remembered");

        new NpgsqlConnectionStringBuilder(_database.ConnectionString)
            .Database.Should().NotBe("acme");
    }

    /// <summary>
    /// <b>The seed is the second thing worth proving</b> (ADR-064 §4). A
    /// database that migrates and does not seed would fail application-tier
    /// tests for a reason that has nothing to do with the code under test.
    /// </summary>
    [Fact]
    public async Task The_real_initializer_chain_ran_against_it()
    {
        await using var context = _database.NewContext();

        // Tenants are seeded by the only initializer this codebase starts
        // with; present means the chain ran. Unfiltered by design — the
        // platform tier, not tenant-owned data.
        (await context.Tenants.CountAsync()).Should().BeGreaterThan(0);
    }
}
