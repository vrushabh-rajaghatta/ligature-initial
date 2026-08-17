using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Acme.Api.Authentication;
using Acme.Api.Development;
using Acme.Api.Provisioning;
using Acme.Api.Endpoints.Authentication;
using Acme.Api.Endpoints.Documents;
using Acme.Api.Endpoints.Platform;
using Acme.Api.Middleware;
using Acme.DocumentManagement.Application;
using Acme.DocumentManagement.Infrastructure;
using Acme.Persistence;
using Acme.Persistence.Initialization;
using Acme.Platform.Application;
using Acme.Platform.Application.Services;
using Acme.Platform.Infrastructure;

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

builder.Services.AddHttpContextAccessor();

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

    // The two ways a deployment gets its first account, and they are mutually
    // exclusive on purpose. Development takes the known-password account;
    // everywhere else the first administrator is invited and chooses their own
    // (ADR-066 decision 5). Guarded here rather than inside either seeder so
    // that "a known password exists only in Development" is readable at the
    // call site.
    if (app.Environment.IsDevelopment())
    {
        await DevelopmentCredentialSeeder.SeedAsync(scope.ServiceProvider);
    }
    else
    {
        await AdministratorSeeder.SeedAsync(
            scope.ServiceProvider, builder.Configuration);
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
