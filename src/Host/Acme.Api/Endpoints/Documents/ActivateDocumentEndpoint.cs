using Acme.DocumentManagement.Application.Commands.ActivateDocument;
using Acme.DocumentManagement.Domain.Aggregates.Documents;

namespace Acme.Api.Endpoints.Documents;

public static class ActivateDocumentEndpoint
{
    public static IEndpointRouteBuilder MapActivateDocument(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/documents/{documentId:guid}/activate",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid documentId,
        ActivateDocumentHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ActivateDocumentCommand(new DocumentId(documentId)),
            cancellationToken);

        return Results.NoContent();
    }
}
