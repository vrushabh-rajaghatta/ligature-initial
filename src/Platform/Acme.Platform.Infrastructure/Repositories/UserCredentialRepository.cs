using Microsoft.EntityFrameworkCore;

using Acme.Persistence;
using Acme.Platform.Domain.Aggregates.User;
using Acme.Platform.Domain.Aggregates.UserCredential;
using Acme.Platform.Contracts;

using UserCredentialAggregate =
    Acme.Platform.Domain.Aggregates.UserCredential.UserCredential;

namespace Acme.Platform.Infrastructure.Repositories;

public sealed class UserCredentialRepository : IUserCredentialRepository
{
    private readonly AcmeDbContext _dbContext;

    public UserCredentialRepository(AcmeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        UserCredentialAggregate credential,
        CancellationToken cancellationToken)
    {
        await _dbContext.UserCredentials.AddAsync(credential, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserCredentialAggregate?> GetByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken)
        => await _dbContext.UserCredentials
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

    public async Task UpdateAsync(
        UserCredentialAggregate credential,
        CancellationToken cancellationToken)
    {
        _dbContext.UserCredentials.Update(credential);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
