using Microsoft.AspNetCore.Mvc;

using Acme.DocumentManagement.Application.Commands.UploadDocument;

namespace Acme.Api.Endpoints.Documents;

public static class UploadDocumentEndpoint
{
    public static IEndpointRouteBuilder MapUploadDocument(
        this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/documents", HandleAsync)
            // API upload consumed by our SPA; no browser antiforgery token.
            .DisableAntiforgery();

        return app;
    }

    private static async Task<IResult> HandleAsync(
        IFormFile file,
        [FromForm] string name,
        UploadDocumentHandler handler,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();

        var result = await handler.HandleAsync(
            new UploadDocumentCommand(
                name,
                file.FileName,
                file.ContentType,
                stream),
            cancellationToken);

        return Results.Created(
            $"/api/documents/{result.Id.Value}",
            new UploadDocumentResponse(result.Id.Value));
    }
}
