using FluentAssertions;

namespace Acme.Architecture.Tests;

/// <summary>
/// SC-003 — a query folder holds a query record beside its handler.
///
/// Queries mirror commands: the record names the question and carries its
/// parameters, the handler answers it. Passing loose parameters straight to
/// HandleAsync works, but it means a query's shape lives in a method
/// signature where the next parameter gets appended without anyone noticing.
///
/// This is the convention Product and Platform were built on. Most contexts
/// written since dropped it, so this list is long — it is a backlog, and the
/// rule holds for everything written from here.
/// </summary>
public sealed class QueryConventionTests
{
    /// <summary>
    /// Query folders that have no query record yet. Shrink, never grow.
    ///
    /// Adding one is a small, self-contained change: define the record, take
    /// it as the handler parameter, construct it at the endpoint. Best done
    /// when you are already in that slice for another reason.
    /// </summary>
    /// <remarks>
    /// <b>Empty, and it stays that way.</b> Both entries were
    /// <c>GetTenants</c> and <c>GetTenantUsers</c>, and ADR-066 deleted the
    /// folders rather than retiring the exemptions — which is the only other
    /// way a list like this is allowed to shrink. A new entry to unblock new
    /// code defeats the mechanism entirely.
    /// </remarks>
    private static readonly HashSet<string> Grandfathered = [];

    [Fact]
    public void Every_query_folder_declares_a_query_record()
    {
        var offenders = QueryFolders()
            .Where(folder => !HasQueryRecord(folder))
            .Select(Repo.Relative)
            .Where(path => !Grandfathered.Contains(path))
            .ToList();

        offenders.Should().BeEmpty(
            "a query folder holds <Name>Query.cs beside <Name>Handler.cs "
            + "(slice-conventions.md SC-003), the same way a command folder "
            + "holds <Name>Command.cs beside its handler");
    }

    [Fact]
    public void The_grandfathered_list_contains_no_stale_entries()
    {
        var outstanding = QueryFolders()
            .Where(folder => !HasQueryRecord(folder))
            .Select(Repo.Relative)
            .ToHashSet();

        var stale = Grandfathered.Where(path => !outstanding.Contains(path)).ToList();

        stale.Should().BeEmpty(
            "these query folders now have a query record, or no longer exist "
            + "— delete them from QueryConventionTests.Grandfathered");
    }

    /// <summary>
    /// A query folder is one holding a handler. The intermediate grouping
    /// folders some contexts use (Queries/Sites/, Queries/Blueprint/) hold no
    /// handler of their own and are skipped.
    ///
    /// Detection is by declared class, not filename: a folder whose handlers
    /// are bundled into ContactQueries.cs is still a query folder, and was
    /// invisible to this test while it matched on "*Handler.cs".
    /// </summary>
    private static IEnumerable<string> QueryFolders() =>
        Repo.Directories("src")
            .Where(folder => folder.Replace('\\', '/')
                .Contains("/Queries/", StringComparison.Ordinal))
            .Where(folder => Directory
                .EnumerateFiles(folder, "*.cs", SearchOption.TopDirectoryOnly)
                .Any(file => Handlers.DeclaredIn(Repo.CodeOf(file)).Any()));

    private static bool HasQueryRecord(string folder) =>
        Directory
            .EnumerateFiles(folder, "*Query.cs", SearchOption.TopDirectoryOnly)
            .Any();
}
