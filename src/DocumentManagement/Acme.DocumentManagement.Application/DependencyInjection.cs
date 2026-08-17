using Microsoft.Extensions.DependencyInjection;

using Acme.DocumentManagement.Application.Commands.ActivateDocument;
using Acme.DocumentManagement.Application.Commands.ArchiveDocument;
using Acme.DocumentManagement.Application.Commands.UploadDocument;
using Acme.DocumentManagement.Application.Commands.UploadDocumentVersion;
using Acme.DocumentManagement.Application.Queries.GetDocument;
using Acme.DocumentManagement.Application.Queries.ListDocuments;

namespace Acme.DocumentManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddDocumentManagementApplication(
        this IServiceCollection services)
    {
        services.AddScoped<UploadDocumentHandler>();

        services.AddScoped<UploadDocumentVersionHandler>();

        services.AddScoped<ActivateDocumentHandler>();

        services.AddScoped<ArchiveDocumentHandler>();

        services.AddScoped<ListDocumentsHandler>();

        services.AddScoped<GetDocumentHandler>();

        return services;
    }
}
