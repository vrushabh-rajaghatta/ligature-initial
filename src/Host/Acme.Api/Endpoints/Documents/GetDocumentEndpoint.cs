using Acme.DocumentManagement.Application.Queries.GetDocument;
using Acme.DocumentManagement.Domain.Aggregates.Documents;

namespace Acme.Api.Endpoints.Documents;

public static class GetDocumentEndpoint
{
    public static IEndpointRouteBuilder MapGetDocument(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/documents/{documentId:guid}", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid documentId,
        GetDocumentHandler handler,
        CancellationToken cancellationToken)
    {
        var document = await handler.HandleAsync(
            new GetDocumentQuery(new DocumentId(documentId)),
            cancellationToken);

        return document is null
            ? Results.NotFound()
            : Results.Ok(document);
    }
}
