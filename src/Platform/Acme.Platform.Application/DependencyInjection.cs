using Microsoft.Extensions.DependencyInjection;

using Acme.Platform.Application.Commands.ActivateUser;
using Acme.Platform.Application.Commands.DeactivateUser;
using Acme.Platform.Application.Commands.InviteUser;
using Acme.Platform.Application.Authentication;
using Acme.Platform.Application.Invitations;
using Acme.Platform.Application.Commands.AcceptInvitation;
using Acme.Platform.Application.Commands.ActivateTenant;
using Acme.Platform.Application.Commands.CreateTenant;
using Acme.Platform.Application.Commands.DeactivateTenant;
using Acme.Platform.Application.Commands.Login;
using Acme.Platform.Application.Commands.Logout;
using Acme.Platform.Application.Commands.RefreshSession;
using Acme.Platform.Application.Commands.ChangePassword;
using Acme.Platform.Application.Commands.CompletePasswordReset;
using Acme.Platform.Application.Commands.RequestPasswordReset;
using Acme.Platform.Application.Commands.RenameTenant;
using Acme.Platform.Application.Commands.ResendInvitation;
using Acme.Platform.Application.PasswordResets;
using Acme.Platform.Application.Commands.SetUserPassword;
using Acme.Platform.Application.Commands.UpdateUserProfile;
using Acme.Platform.Application.Commands.RevokeSession;
using Acme.Platform.Application.Queries.GetSessions;
using Acme.Platform.Application.Queries.GetTenants;
using Acme.Platform.Application.Queries.GetTenantUsers;
using Acme.Platform.Application.Queries.GetUserById;
using Acme.Platform.Application.Queries.GetUsers;

namespace Acme.Platform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformApplication(
        this IServiceCollection services)
    {
        services.AddScoped<InvitationIssuer>();

        services.AddScoped<InviteUserHandler>();

        services.AddScoped<ResendInvitationHandler>();

        services.AddScoped<AcceptInvitationHandler>();

        services.AddScoped<ActivateUserHandler>();

        services.AddScoped<DeactivateUserHandler>();

        services.AddScoped<UpdateUserProfileHandler>();

        services.AddScoped<SetUserPasswordHandler>();

        services.AddScoped<PasswordResetIssuer>();

        services.AddScoped<RequestPasswordResetHandler>();

        services.AddScoped<CompletePasswordResetHandler>();

        // Stateless: it composes two issuers and holds nothing per request.
        services.AddSingleton<SessionFactory>();

        // Scoped, unlike SessionFactory: these hold repositories.
        services.AddScoped<SessionRevoker>();

        services.AddScoped<CredentialTrustRevoker>();

        services.AddScoped<ChangePasswordHandler>();

        services.AddScoped<GetSessionsHandler>();

        services.AddScoped<RevokeSessionHandler>();

        services.AddScoped<LoginHandler>();

        services.AddScoped<RefreshSessionHandler>();

        services.AddScoped<LogoutHandler>();

        services.AddScoped<CreateTenantHandler>();

        services.AddScoped<RenameTenantHandler>();

        services.AddScoped<ActivateTenantHandler>();

        services.AddScoped<DeactivateTenantHandler>();

        services.AddScoped<GetTenantsHandler>();

        services.AddScoped<GetTenantUsersHandler>();

        services.AddScoped<GetUsersHandler>();

        services.AddScoped<GetUserByIdHandler>();

        return services;
    }
}
