using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Acme.Api.Authentication;
using Acme.Api.Development;
using Acme.Api.Endpoints.Authentication;
using Acme.Api.Endpoints.Documents;
using Acme.Api.Endpoints.Platform;
using Acme.Api.Endpoints.PlatformAdministration;
using Acme.Api.Middleware;
using Acme.Api.Tenancy;
using Acme.DocumentManagement.Application;
using Acme.DocumentManagement.Infrastructure;
using Acme.Persistence;
using Acme.Persistence.Initialization;
using Acme.Platform.Application;
using Acme.Platform.Application.Services;
using Acme.Platform.Infrastructure;
using Acme.SharedKernel.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

const string DevCorsPolicy = "DevCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(DevCorsPolicy, policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            // Required for the session cookies to travel at all: the browser
            // withholds them from cross-origin requests otherwise. Only legal
            // beside a specific origin, never AllowAnyOrigin.
            .AllowCredentials();
    });
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter());
});

// Tenant context is scoped: one per request, resolved from the authenticated
// caller. Registered before the modules so every handler can depend on it.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, ClaimsTenantContext>();

builder.Services.AddAcmeAuthentication();

builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddPlatformApplication();
builder.Services.AddPlatformInfrastructure(builder.Configuration);

// Development only, and guarded here rather than inside the notifier: it writes
// a live acceptance token to the log, and that guarantee should be readable
// where it is wired up. Registered after the default so it replaces it.
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<
        IInvitationNotifier, DevelopmentInvitationNotifier>();

    builder.Services.AddScoped<
        IPasswordResetNotifier, DevelopmentPasswordResetNotifier>();
}

builder.Services.AddDocumentManagementApplication();
builder.Services.AddDocumentManagementInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider
        .GetRequiredService<AcmeDbContext>().Database;

    // The seeders below are insert-if-empty and assume their tables exist, so
    // an unmigrated database would fail with a raw 42P01 forty frames deep
    // rather than saying what to do about it. Both branches fix that; they
    // differ on who may change the schema.
    //
    // Configuration rather than the environment name, because "may this process
    // alter the schema?" is a property of the deployment and not of the word it
    // was labelled with. It is false when absent (appsettings.json says so out
    // loud) so that forgetting the setting is the safe outcome, and the whole of
    // it is one key: Database:MigrateOnStartup.
    if (builder.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
    {
        await database.MigrateAsync();
    }
    else
    {
        // Migrating is then a deployment step and not a side effect of booting.
        // Three reasons, none of them style: instances starting together would
        // race one another, a long migration would hold the process before it
        // could report healthy, and the alternative grants the application's own
        // credentials the right to alter the schema for the entire time it runs.
        // The supported artifact is `dotnet ef migrations script --idempotent`.
        var pending = (await database.GetPendingMigrationsAsync()).ToList();

        if (pending.Count > 0)
        {
            throw new InvalidOperationException(
                $"The database is {pending.Count} migration(s) behind: "
                + string.Join(", ", pending)
                + ". Apply them as part of the deployment, or set "
                + "Database:MigrateOnStartup to true if this process is meant "
                + "to own the schema.");
        }
    }

    var initializers = scope.ServiceProvider.GetServices<IDataInitializer>();

    foreach (var initializer in initializers)
    {
        await initializer.InitializeAsync();
    }

    // Development only, and deliberately guarded here rather than inside the
    // seeders: accounts with known passwords must never be created anywhere
    // else, and that guarantee should be readable at the call site.
    if (app.Environment.IsDevelopment())
    {
        await DevelopmentCredentialSeeder.SeedAsync(scope.ServiceProvider);
        await PlatformAdministratorSeeder.SeedAsync(scope.ServiceProvider);
    }
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(DevCorsPolicy);
}

// After CORS so a rejected preflight never reaches the token handler, and
// before the endpoints so RequireAuthorization has an identity to inspect.
app.UseAuthentication();
app.UseAuthorization();

var authentication = app.MapGroup("").WithTags("Authentication");
authentication.MapLogin();
authentication.MapRefreshSession();
authentication.MapLogout();
authentication.MapAcceptInvitation();
authentication.MapRequestPasswordReset();
authentication.MapCompletePasswordReset();
authentication.MapChangePassword();
authentication.MapSessions();
authentication.MapGetCurrentUser();

var platformAdministration = app.MapGroup("").WithTags("Platform Administration");
platformAdministration.MapTenantAdministration();

var users = app.MapGroup("").WithTags("Users");
users.MapInviteUser();
users.MapResendInvitation();
users.MapListUsers();
users.MapGetUser();
users.MapUpdateUserProfile();
users.MapActivateUser();
users.MapDeactivateUser();

var documents = app.MapGroup("").WithTags("Documents");
documents.MapUploadDocument();
documents.MapUploadDocumentVersion();
documents.MapListDocuments();
documents.MapGetDocument();
documents.MapActivateDocument();
documents.MapArchiveDocument();

app.Run();

/// <summary>
/// Exposed so the integration tests can host this exact application through
/// <c>WebApplicationFactory</c>. Nothing else references it: the tests must
/// exercise the real pipeline — authentication handler, middleware order,
/// cookie writing — rather than a rebuilt approximation of it.
/// </summary>
public partial class Program;
