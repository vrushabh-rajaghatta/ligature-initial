using Acme.Platform.Domain.ValueObjects;
using Acme.SharedKernel.Abstractions;
using Acme.SharedKernel.Exceptions;
using Acme.Platform.Contracts;

namespace Acme.Platform.Domain.Aggregates.User;

/// <summary>
/// A person who can access this deployment of Acme. This is the business
/// concept of a person — not an authentication account; passwords, roles,
/// permissions and sign-in are separate concerns owned elsewhere.
/// </summary>
/// <remarks>
/// Carried a <c>TenantId</c> until ADR-066, along with two factories that kept
/// "tenant user without a tenant" unexpressible. A deployment now serves one
/// customer, so every user belongs to it by construction and the distinction
/// has nothing left to express — both factories collapse into
/// <see cref="Create"/>.
/// </remarks>
public sealed class User : AggregateRoot<UserId>
{
    private User(
        UserId id,
        UserRole role,
        Email email,
        string firstName,
        string lastName,
        DateTime createdOn)
    {
        Id = id;
        Role = role;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        Status = UserStatus.Invited;
        CreatedOn = createdOn;
    }

    /// <summary>What this user administers (ADR-033, narrowed by ADR-066).</summary>
    public UserRole Role { get; private set; }

    public Email Email { get; private set; }

    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public UserStatus Status { get; private set; }

    public DateTime CreatedOn { get; private set; }

    /// <summary>
    /// Invites a new user: creates them in the <see cref="UserStatus.Invited"/>
    /// state, pending acceptance. The Email value object already guarantees a
    /// valid, normalized address; the aggregate enforces the rest.
    /// </summary>
    public static User Create(
        Email email,
        string firstName,
        string lastName,
        UserRole role = UserRole.Member)
    {
        if (email is null)
            throw new DomainException(UserErrors.EmailRequired);

        return new User(
            UserId.New(),
            role,
            email,
            RequireName(firstName, UserErrors.FirstNameRequired),
            RequireName(lastName, UserErrors.LastNameRequired),
            DateTime.UtcNow);
    }

    /// <summary>Updates the user's name. No-op when the name is unchanged.</summary>
    public void ChangeName(string firstName, string lastName)
    {
        var newFirstName = RequireName(firstName, UserErrors.FirstNameRequired);
        var newLastName = RequireName(lastName, UserErrors.LastNameRequired);

        if (newFirstName == FirstName && newLastName == LastName)
            return;

        FirstName = newFirstName;
        LastName = newLastName;
    }

    /// <summary>Changes the user's email. No-op when the email is unchanged.</summary>
    public void ChangeEmail(Email email)
    {
        if (email is null)
            throw new DomainException(UserErrors.EmailRequired);

        if (email == Email)
            return;

        Email = email;
    }

    /// <summary>Invited/Inactive -> Active. Idempotent when already active.</summary>
    public void Activate()
    {
        if (Status == UserStatus.Active)
            return;

        Status = UserStatus.Active;
    }

    /// <summary>Active/Invited -> Inactive. Idempotent when already inactive.</summary>
    public void Deactivate()
    {
        if (Status == UserStatus.Inactive)
            return;

        Status = UserStatus.Inactive;
    }

    private static string RequireName(string value, string error)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(error);

        return value.Trim();
    }
}
