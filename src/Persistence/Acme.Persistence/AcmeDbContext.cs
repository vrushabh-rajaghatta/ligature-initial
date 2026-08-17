using Microsoft.EntityFrameworkCore;

using Acme.DocumentManagement.Domain.Aggregates.Documents;

using UserAggregate =
    Acme.Platform.Domain.Aggregates.User.User;
using UserCredentialAggregate =
    Acme.Platform.Domain.Aggregates.UserCredential.UserCredential;
using RefreshTokenAggregate =
    Acme.Platform.Domain.Aggregates.RefreshToken.RefreshToken;
using InvitationAggregate =
    Acme.Platform.Domain.Aggregates.Invitation.Invitation;
using PasswordResetAggregate =
    Acme.Platform.Domain.Aggregates.PasswordReset.PasswordReset;
using SessionAggregate =
    Acme.Platform.Domain.Aggregates.Session.Session;

namespace Acme.Persistence;

/// <summary>
/// The one context, over the one database this deployment owns.
/// </summary>
/// <remarks>
/// <b>There are no global query filters, and that is the decision, not an
/// omission</b> (ADR-066). Isolation between customers is the database
/// boundary: a deployment is connected to exactly one customer's database, so
/// there is no other customer's row for a query to reach. The
/// <c>ITenantContext</c> this class used to take, the two current-tenant
/// accessors it read, and <c>ApplyTenantFilters</c> are all gone with the
/// concept.
/// <para>
/// If a second customer ever has to be served by one deployment, this is the
/// file that cannot express it — see ADR-066 § Revisit When before adding a
/// filter back.
/// </para>
/// </remarks>
public sealed class AcmeDbContext : DbContext
{
    public AcmeDbContext(DbContextOptions<AcmeDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserAggregate> Users =>
        Set<UserAggregate>();

    public DbSet<UserCredentialAggregate> UserCredentials =>
        Set<UserCredentialAggregate>();

    public DbSet<RefreshTokenAggregate> RefreshTokens =>
        Set<RefreshTokenAggregate>();

    public DbSet<InvitationAggregate> Invitations =>
        Set<InvitationAggregate>();

    public DbSet<PasswordResetAggregate> PasswordResets =>
        Set<PasswordResetAggregate>();

    public DbSet<SessionAggregate> Sessions =>
        Set<SessionAggregate>();

    /// <summary>This deployment's managed documents.</summary>
    /// <remarks>
    /// <b>There is deliberately no <c>DocumentVersions</c> set.</b> A version
    /// is a child of the <c>Document</c> aggregate and has no independent
    /// lifecycle, so exposing it as a root would invite reads and writes that
    /// bypass the root that owns its numbering (ADR-016). Every version read
    /// starts here. This rule predates ADR-066 and outlived its original
    /// tenant-leak justification on aggregate discipline alone.
    /// </remarks>
    public DbSet<Document> Documents =>
        Set<Document>();

    /// <summary>Read-only projection over Users for the user directory.</summary>
    public DbSet<ReadModels.UserDirectoryRow> UserDirectory =>
        Set<ReadModels.UserDirectoryRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AcmeDbContext).Assembly);
    }
}
