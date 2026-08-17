using Microsoft.EntityFrameworkCore;

using Acme.Persistence;
using Acme.Platform.Application.Common;

namespace Acme.Platform.Application.Queries.GetUsers;

/// <summary>
/// Reads the user directory straight from the database. This is reporting, not
/// domain modelling: no repository, no aggregate loading, no tracking, no
/// Include — only the columns the directory screen needs, projected from a flat
/// read model rather than through the User aggregate's value converters.
/// </summary>
public sealed class GetUsersHandler
{
    private readonly AcmeDbContext _dbContext;

    public GetUsersHandler(AcmeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<UserListItem>> HandleAsync(
        GetUsersQuery query,
        CancellationToken cancellationToken)
    {
        // Clamp rather than reject: a caller asking for page 0 or 5000 rows gets
        // a sensible page, never an unbounded read.
        var page = query.Page < 1 ? GetUsersQuery.DefaultPage : query.Page;
        var pageSize = Math.Clamp(
            query.PageSize, 1, GetUsersQuery.MaxPageSize);

        // Filtered by tenant until ADR-066. The directory is now every user in
        // this deployment, which is the same set the filter used to produce.
        var users = _dbContext.UserDirectory
            .AsNoTracking();

        if (query.Status is not null)
        {
            var status = query.Status.Value;
            users = users.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            // One search box across first name, last name and email.
            var pattern = $"%{query.Search.Trim()}%";

            users = users.Where(x =>
                EF.Functions.ILike(x.FirstName, pattern)
                || EF.Functions.ILike(x.LastName, pattern)
                || EF.Functions.ILike(x.Email, pattern));
        }

        var totalCount = await users.CountAsync(cancellationToken);

        var items = await users
            // Paged: a tie here would move a user between pages, so the id
            // is not decoration.
            .OrderByDescending(x => x.CreatedOn) // newest invitations first
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserListItem(
                x.Id,
                x.FirstName,
                x.LastName,
                x.Email,
                x.Status,
                x.CreatedOn))
            .ToListAsync(cancellationToken);

        return new PagedResult<UserListItem>(
            items, totalCount, page, pageSize);
    }
}
