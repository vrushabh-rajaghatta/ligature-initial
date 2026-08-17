using Acme.Platform.Domain.Aggregates.User;
using Acme.SharedKernel.Exceptions;

using UserAggregate = Acme.Platform.Domain.Aggregates.User.User;
using Acme.Platform.Contracts;

namespace Acme.Platform.Application.Common;

internal static class UserRepositoryExtensions
{
    /// <summary>
    /// Loads a user that must exist. Extracted once three commands needed the
    /// identical lookup (update profile, activate, deactivate).
    /// </summary>
    /// <remarks>
    /// Took a <c>TenantId</c> until ADR-066 and rejected users belonging to
    /// another tenant as not found. Every user in this database belongs to
    /// this deployment, so the check has nothing left to compare — the
    /// not-found path now only means the user does not exist.
    /// </remarks>
    public static async Task<UserAggregate> GetRequiredAsync(
        this IUserRepository repository,
        UserId userId,
        CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync(userId, cancellationToken);

        return user ?? throw new NotFoundException(PlatformErrors.UserNotFound);
    }
}
