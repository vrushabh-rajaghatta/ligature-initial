using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Acme.Persistence.Initialization;
using Acme.Persistence.Initialization.Platform;

namespace Acme.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
        => services.AddPersistence(configuration.GetConnectionString("Acme"));

    /// <summary>
    /// The same registration, for a caller that already holds the connection
    /// string rather than a configuration to read it from.
    /// </summary>
    /// <remarks>
    /// Exists for <c>Acme.TestSupport</c>, which provisions a database per test
    /// assembly (<see href="../../../docs/adr/ADR-064-the-test-suite-provisions-its-own-schema.md">ADR-064</see>)
    /// and therefore knows its connection string before any configuration
    /// exists. <b>An overload rather than a second registration list</b>: the
    /// initializer order is load-bearing once seeds reference one another, and
    /// a test path that assembled its own copy would silently stop running
    /// whichever initializer was added next.
    /// </remarks>
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        string? connectionString)
    {
        services.AddDbContext<AcmeDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IDataInitializer, TenantInitializer>();

        return services;
    }
}
