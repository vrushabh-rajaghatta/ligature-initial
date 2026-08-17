using Microsoft.EntityFrameworkCore;

using Acme.TestSupport;

namespace Acme.Platform.Application.Tests;

/// <summary>
/// This assembly's database — created from the current migration chain, seeded
/// by the real initializers, dropped when the assembly's tests finish
/// (<see href="../../../docs/adr/ADR-064-the-test-suite-provisions-its-own-schema.md">ADR-064</see>).
/// </summary>
/// <remarks>
/// A one-line subclass rather than the base type directly, so that
/// <c>GetType().Assembly</c> inside <see cref="AcmeTestDatabase"/> names
/// <em>this</em> assembly and the database is named after it.
/// </remarks>
public sealed class PlatformDatabase : AcmeTestDatabase
{
    public const string Collection = "Platform database";
}

/// <summary>
/// Puts every database-touching class in this assembly on one shared database.
/// </summary>
/// <remarks>
/// <b>They therefore do not run in parallel with each other</b>, which is the
/// price of one database per assembly and is worth naming rather than
/// discovering. It is also what the hand-written cleanup in these tests has
/// always assumed.
/// </remarks>
[CollectionDefinition(PlatformDatabase.Collection)]
public sealed class PlatformDatabaseCollection : ICollectionFixture<PlatformDatabase>;

/// <summary>
/// Empties every person-scoped table, children first.
/// </summary>
/// <remarks>
/// The directory queries used to scope themselves by <c>TenantId</c>, which
/// isolated each test class's rows from every other class sharing this
/// assembly's database (ADR-064 §2). ADR-066 removed the column, so a class
/// that asserts on a total count has to own the table outright instead.
/// <para>
/// Safe because every database-touching class here is in one xUnit collection
/// and therefore runs serially — a class has the database to itself between
/// its own <c>InitializeAsync</c> and <c>DisposeAsync</c>.
/// </para>
/// </remarks>
public static class UserTables
{
    public static async Task ClearAsync(Acme.Persistence.AcmeDbContext context)
    {
        foreach (var table in new[]
                 {
                     "RefreshTokens", "Sessions", "UserCredentials",
                     "Invitations", "PasswordResets", "Users"
                 })
        {
            await context.Database.ExecuteSqlRawAsync($"DELETE FROM \"{table}\"");
        }
    }
}
