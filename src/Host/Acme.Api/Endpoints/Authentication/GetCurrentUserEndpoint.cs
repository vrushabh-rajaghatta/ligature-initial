using Acme.Platform.Application.Services;
using Acme.Platform.Contracts;

namespace Acme.Api.Endpoints.Authentication;

public static class GetCurrentUserEndpoint
{
    public static IEndpointRouteBuilder MapGetCurrentUser(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/auth/me",
            Handle)
        .RequireAuthorization()
        .WithName("GetCurrentUser")
        .WithSummary("The user behind the current access token")
        .WithTags("Authentication");

        return app;
    }

    // Identity comes from the token and nowhere else. Reported a tenant
    // alongside it until ADR-066; which customer this is, is now the
    // deployment being asked.
    private static IResult Handle(ICurrentUser currentUser) =>
        Results.Ok(new CurrentUserResponse(
            currentUser.UserId.Value,
            currentUser.Email.Value,
            currentUser.Role.ToString()));
}
