namespace Acme.DocumentManagement.Application.Commands.UploadDocument;

public sealed record UploadDocumentCommand(
    string Name,
    string OriginalFileName,
    string ContentType,
    Stream Content);
