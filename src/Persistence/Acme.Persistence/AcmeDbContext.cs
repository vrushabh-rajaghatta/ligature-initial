using Microsoft.EntityFrameworkCore;

using Acme.DocumentManagement.Domain.Aggregates.Documents;
using Acme.SharedKernel.Abstractions;
using Acme.SharedKernel.Primitives;

using UserAggregate =
    Acme.Platform.Domain.Aggregates.User.User;
using TenantAggregate =
    Acme.Platform.Domain.Aggregates.Tenant.Tenant;
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

public sealed class AcmeDbContext : DbContext
{
    private readonly ITenantContext? _tenantContext;

    /// <summary>
    /// The tenant context is optional so the context can exist before a
    /// request does (design-time tools, direct construction in tests). The
    /// filters treat a missing context exactly like a missing identity:
    /// no tenant, no rows. Fail closed, never open.
    /// </summary>
    public AcmeDbContext(
        DbContextOptions<AcmeDbContext> options,
        ITenantContext? tenantContext = null)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Read per query execution, not captured at model build: the filter
    /// expressions reference this instance member, which EF lifts to a SQL
    /// parameter — so the compiled model is shared while the tenant is always
    /// the current request's. Never <c>Guid.Empty</c>: a strongly typed id
    /// cannot hold it, so "no tenant" is null and only null.
    /// </summary>
    private TenantId? CurrentTenant =>
        _tenantContext?.TenantIdOrNull;

    private Guid? CurrentTenantGuid =>
        _tenantContext?.TenantIdOrNull is { } tenant
            ? tenant.Value
            : null;

    public DbSet<TenantAggregate> Tenants =>
        Set<TenantAggregate>();

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

    /// <summary>
    /// The tenant's managed documents. Tenant-owned and fail-closed
    /// (ADR-031).
    /// </summary>
    /// <remarks>
    /// <b>There is deliberately no <c>DocumentVersions</c> set.</b> A version
    /// carries no <c>TenantId</c> and therefore has no query filter of its
    /// own, so <c>Set&lt;DocumentVersion&gt;()</c> exposed as a root would
    /// read every tenant's versions. Every read of a version starts here, at
    /// the filtered root.
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

        ApplyTenantFilters(modelBuilder);
    }

    /// <summary>
    /// Tenant isolation, enforced once, here, for every tenant-owned entity
    /// (ADR-031). A handler that forgets its <c>.Where</c> now returns the
    /// caller's rows instead of everyone's.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every filter starts with an explicit null guard rather than relying on
    /// SQL null semantics. The distinction matters for <c>Users</c>: with a
    /// null tenant, a bare <c>u.TenantId == CurrentTenant</c> would translate
    /// to <c>"TenantId" IS NULL</c> — which matches every platform user. The
    /// guard makes "no identity" mean <em>no rows</em>, not "the null-tenant
    /// rows".
    /// </para>
    /// <para>
    /// Tenant filtering has <b>three shapes</b> (ADR-038), and choosing
    /// between them is the decision every new entity faces:
    /// <list type="number">
    /// <item><b>Fail-closed tenant-owned</b> — <c>x.TenantId == CurrentTenant</c>.
    /// The tenant owns the data. <c>Users</c>, <c>Documents</c>.</item>
    /// <item><b>Shared plus extensible</b> — <c>TenantId == null || == CurrentTenant</c>.
    /// The platform ships a baseline the tenant may extend. None yet in this
    /// codebase — the shape arrives with the first shared catalogue.</item>
    /// <item><b>Global world facts</b> — no filter. Data describing an
    /// external reality that does not differ by tenant. None yet.</item>
    /// </list>
    /// Also unfiltered, for different reasons: <c>Tenants</c> (the platform
    /// tier), and the person-scoped satellites (<c>UserCredentials</c>,
    /// <c>RefreshTokens</c>, <c>Invitations</c>, <c>PasswordResets</c>,
    /// <c>Sessions</c>), which carry no tenant and are reachable only by user
    /// id or token hash. Child entities (<c>DocumentVersions</c>) are
    /// reachable only through a filtered root.
    /// </para>
    /// </remarks>
    private void ApplyTenantFilters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAggregate>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        modelBuilder.Entity<Document>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        // The ToView read models map onto the same physical tables but are
        // different CLR types — the aggregate filters above do NOT propagate
        // to them. Left unfiltered they would be a ready-made leak path.
        modelBuilder.Entity<ReadModels.UserDirectoryRow>().HasQueryFilter(
            x => CurrentTenantGuid != null && x.TenantId == CurrentTenantGuid);
    }
}
