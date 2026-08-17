using Acme.DocumentManagement.Application.Queries.ListDocuments;

namespace Acme.Api.Endpoints.Documents;

public static class ListDocumentsEndpoint
{
    public static IEndpointRouteBuilder MapListDocuments(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/documents", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        ListDocumentsHandler handler,
        CancellationToken cancellationToken)
    {
        var documents = await handler.HandleAsync(
            new ListDocumentsQuery(),
            cancellationToken);

        return Results.Ok(documents);
    }
}
