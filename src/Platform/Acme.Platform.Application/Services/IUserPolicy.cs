using Acme.Platform.Domain.Aggregates.User;
using Acme.Platform.Domain.ValueObjects;
using Acme.Platform.Contracts;

namespace Acme.Platform.Application.Services;

/// <summary>
/// Business rules for users that require asking the outside world — distinct from
/// the aggregate's own invariants, which live in the domain. Worded as
/// <c>Ensure…</c> so the handler reads as intent: the method either lets the flow
/// continue or throws a <see cref="Exceptions.BusinessRuleViolationException"/>.
/// </summary>
public interface IUserPolicy
{
    /// <summary>
    /// An email address identifies exactly one user in this deployment
    /// (ADR-021, as narrowed by ADR-066).
    /// </summary>
    Task EnsureEmailIsUniqueAsync(
        Email email,
        CancellationToken cancellationToken);

    /// <summary>
    /// Same rule as <see cref="EnsureEmailIsUniqueAsync"/>, but ignores the user
    /// being updated — otherwise saving a profile without changing the email
    /// would collide with itself.
    /// </summary>
    Task EnsureEmailIsUniqueForUpdateAsync(
        UserId userId,
        Email email,
        CancellationToken cancellationToken);
}
