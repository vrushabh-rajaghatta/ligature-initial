using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Acme.DocumentManagement.Domain.Aggregates.Documents;
using Acme.DocumentManagement.Infrastructure.Repositories;
using Acme.Storage;

namespace Acme.DocumentManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDocumentManagementInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IDocumentRepository, DocumentRepository>();

        var rootPath = configuration["Storage:RootPath"];
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            rootPath = "storage";
        }

        services.AddScoped<IFileStorage>(_ => new LocalFileStorage(rootPath));

        return services;
    }
}
