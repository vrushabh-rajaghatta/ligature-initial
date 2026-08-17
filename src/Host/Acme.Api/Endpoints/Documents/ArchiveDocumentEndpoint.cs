using Acme.DocumentManagement.Application.Commands.ArchiveDocument;
using Acme.DocumentManagement.Domain.Aggregates.Documents;

namespace Acme.Api.Endpoints.Documents;

public static class ArchiveDocumentEndpoint
{
    public static IEndpointRouteBuilder MapArchiveDocument(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/documents/{documentId:guid}/archive",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid documentId,
        ArchiveDocumentHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ArchiveDocumentCommand(new DocumentId(documentId)),
            cancellationToken);

        return Results.NoContent();
    }
}
