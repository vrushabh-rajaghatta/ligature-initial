using Acme.DocumentManagement.Application.Commands.UploadDocumentVersion;
using Acme.DocumentManagement.Domain.Aggregates.Documents;

namespace Acme.Api.Endpoints.Documents;

public static class UploadDocumentVersionEndpoint
{
    public static IEndpointRouteBuilder MapUploadDocumentVersion(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/documents/{documentId:guid}/versions",
                HandleAsync)
            // API upload consumed by our SPA; no browser antiforgery token.
            .DisableAntiforgery();

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid documentId,
        IFormFile file,
        UploadDocumentVersionHandler handler,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();

        var result = await handler.HandleAsync(
            new UploadDocumentVersionCommand(
                new DocumentId(documentId),
                file.FileName,
                file.ContentType,
                stream),
            cancellationToken);

        return Results.Created(
            $"/api/documents/{documentId}",
            new UploadDocumentVersionResponse(
                result.VersionId.Value,
                result.VersionNumber));
    }
}
