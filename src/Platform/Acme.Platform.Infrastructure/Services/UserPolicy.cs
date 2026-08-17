using Microsoft.EntityFrameworkCore;

using Acme.Persistence;
using Acme.Platform.Application;
using Acme.Platform.Application.Services;
using Acme.Platform.Domain.Aggregates.Tenant;
using Acme.Platform.Domain.Aggregates.User;
using Acme.Platform.Domain.ValueObjects;
using Acme.SharedKernel.Exceptions;
using Acme.SharedKernel.Primitives;
using Acme.Platform.Contracts;

namespace Acme.Platform.Infrastructure.Services;

public sealed class UserPolicy : IUserPolicy
{
    private readonly AcmeDbContext _dbContext;

    public UserPolicy(AcmeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsureTenantCanAcceptUsersAsync(
        TenantId tenantId,
        CancellationToken cancellationToken)
    {
        var tenant = await _dbContext.Tenants
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == tenantId, cancellationToken);

        // The tenant the caller named does not exist, so the *request* is
        // wrong (400) rather than the system state being in conflict (409).
        // Matches how RegulatoryApplication classifies the same condition.
        if (tenant is null)
            throw new DomainException(
                PlatformErrors.TenantDoesNotExist);

        if (tenant.Status != TenantStatus.Active)
            throw new BusinessRuleViolationException(
                PlatformErrors.TenantInactive);
    }

    // Both rules are deliberately unscoped by tenant: an email address
    // identifies exactly one user across Acme (ADR-021). IgnoreQueryFilters
    // is that decision carried past the tenant filter — scoped to the caller's
    // tenant, the check would miss a collision in another tenant and the
    // unique index would answer with a 500 instead of this rule's 409.

    public async Task EnsureEmailIsUniqueAsync(
        Email email,
        CancellationToken cancellationToken)
    {
        var alreadyInUse = await _dbContext.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(x => x.Email == email, cancellationToken);

        if (alreadyInUse)
            throw new BusinessRuleViolationException(
                PlatformErrors.EmailAlreadyInUse);
    }

    public async Task EnsureEmailIsUniqueForUpdateAsync(
        UserId userId,
        Email email,
        CancellationToken cancellationToken)
    {
        // Identical to the invite rule, except the user being updated is not
        // allowed to collide with itself.
        var alreadyInUse = await _dbContext.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(
                x => x.Email == email && x.Id != userId,
                cancellationToken);

        if (alreadyInUse)
            throw new BusinessRuleViolationException(
                PlatformErrors.EmailAlreadyInUse);
    }
}
