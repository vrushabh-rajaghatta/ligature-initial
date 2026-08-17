using Acme.Platform.Domain.Aggregates.User;

namespace Acme.Platform.Application.Queries.GetTenantUsers;

public sealed record TenantUserListItem(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    UserStatus Status,
    UserRole Role);
