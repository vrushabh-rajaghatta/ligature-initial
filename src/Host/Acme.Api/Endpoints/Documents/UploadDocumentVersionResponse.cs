namespace Acme.Api.Endpoints.Documents;

public sealed record UploadDocumentVersionResponse(
    Guid VersionId,
    int VersionNumber);
