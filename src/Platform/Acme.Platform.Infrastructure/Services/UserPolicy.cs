using Microsoft.EntityFrameworkCore;

using Acme.Persistence;
using Acme.Platform.Application;
using Acme.Platform.Application.Services;
using Acme.Platform.Domain.Aggregates.User;
using Acme.Platform.Domain.ValueObjects;
using Acme.SharedKernel.Exceptions;
using Acme.Platform.Contracts;

namespace Acme.Platform.Infrastructure.Services;

public sealed class UserPolicy : IUserPolicy
{
    private readonly AcmeDbContext _dbContext;

    public UserPolicy(AcmeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // An email address identifies exactly one user in this deployment
    // (ADR-021, as narrowed by ADR-066). These used IgnoreQueryFilters() to
    // reach past the tenant filter; with no filters left there is nothing to
    // ignore, and the unique index backs the rule either way.

    public async Task EnsureEmailIsUniqueAsync(
        Email email,
        CancellationToken cancellationToken)
    {
        var alreadyInUse = await _dbContext.Users
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
            .AsNoTracking()
            .AnyAsync(
                x => x.Email == email && x.Id != userId,
                cancellationToken);

        if (alreadyInUse)
            throw new BusinessRuleViolationException(
                PlatformErrors.EmailAlreadyInUse);
    }
}
