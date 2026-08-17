using Microsoft.EntityFrameworkCore;

using Acme.Persistence;
using Acme.Platform.Application.Invitations;
using Acme.Platform.Domain.Aggregates.User;
using Acme.Platform.Domain.ValueObjects;

using UserAggregate = Acme.Platform.Domain.Aggregates.User.User;

namespace Acme.Api.Provisioning;

/// <summary>
/// Creates this deployment's first administrator, once, on a fresh database.
/// </summary>
/// <remarks>
/// <para>
/// <b>No password is ever created here</b> (ADR-066 decision 5). The
/// administrator is created <c>Invited</c> and an invitation is issued through
/// the ordinary ADR-027 flow, so they set their own credential from the
/// acceptance link. A password supplied through configuration would be one
/// that every operator, every deployment manifest and every backup of that
/// manifest also holds — which is the reason
/// <see cref="Development.DevelopmentCredentialSeeder"/> is guarded to
/// Development and this is not simply a copy of it.
/// </para>
/// <para>
/// <b>Seeds only when there are no users at all.</b> That is the definition of
/// a fresh deployment, and it is the one idempotency rule that cannot damage a
/// live one: a running customer always has at least the administrator this
/// created, so this never fires twice and never adopts a database it did not
/// start. Rotating a locked-out administrator is an operational task, not this
/// — re-running a seeder is the wrong tool for it, because a seeder that can
/// act on a populated database is a back door.
/// </para>
/// <para>
/// Lives in the Host for the same reason the development seeders do: it needs
/// the application layer, and its "only on a fresh database" guarantee should
/// be readable where it is invoked.
/// </para>
/// </remarks>
public static class AdministratorSeeder
{
    public const string EmailKey = "Administrator:Email";
    public const string FirstNameKey = "Administrator:FirstName";
    public const string LastNameKey = "Administrator:LastName";

    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var dbContext = services.GetRequiredService<AcmeDbContext>();

        // A deployment that already has anyone is not a fresh one.
        if (await dbContext.Users.AnyAsync(cancellationToken)) return;

        var configured = configuration[EmailKey];

        if (string.IsNullOrWhiteSpace(configured))
        {
            // Loud, not silent. A fresh deployment with no administrator has
            // no way in at all — nobody can sign in, and nobody can invite
            // anyone — so booting "successfully" into that state would be a
            // worse outcome than refusing to start.
            throw new InvalidOperationException(
                $"This database has no users and {EmailKey} is not set, so "
                + "the deployment would start with no way for anyone to sign "
                + "in. Set it to the first administrator's email address; "
                + "they receive an invitation and choose their own password.");
        }

        var users = services.GetRequiredService<IUserRepository>();
        var invitations = services.GetRequiredService<InvitationIssuer>();

        var administrator = UserAggregate.Create(
            Email.Create(configured),
            configuration[FirstNameKey] ?? "Account",
            configuration[LastNameKey] ?? "Administrator",
            UserRole.Administrator);

        await users.AddAsync(administrator, cancellationToken);

        // Invited, with no credential. The acceptance link is what turns this
        // into an account somebody can use.
        await invitations.IssueAsync(
            administrator, DateTime.UtcNow, cancellationToken);
    }
}
