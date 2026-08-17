using Acme.Platform.Domain.Aggregates.User;
using Acme.Platform.Contracts;

using UserCredentialAggregate =
    Acme.Platform.Domain.Aggregates.UserCredential.UserCredential;

namespace Acme.Platform.Domain.Aggregates.UserCredential;

/// <summary>
/// Interface in the domain, implementation in infrastructure — matching
/// <see cref="IUserRepository"/>, the convention this bounded context uses.
/// </summary>
public interface IUserCredentialRepository
{
    Task AddAsync(
        UserCredentialAggregate credential,
        CancellationToken cancellationToken);

    Task<UserCredentialAggregate?> GetByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        UserCredentialAggregate credential,
        CancellationToken cancellationToken);
}
