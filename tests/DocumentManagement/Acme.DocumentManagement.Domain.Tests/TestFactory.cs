using Acme.DocumentManagement.Domain.Aggregates.Documents;
using Acme.SharedKernel.Primitives;

namespace Acme.DocumentManagement.Domain.Tests;

internal static class TestFactory
{
    public static Document NewDocument() =>
        Document.Create(TenantId.New(), "Employee Handbook");

    public static void AddInitial(Document document) =>
        document.AddInitialVersion(
            originalFileName: "handbook.pdf",
            storedFileName: "v1.pdf",
            contentType: "application/pdf",
            fileSize: 1024,
            storagePath: "documents/x/v1.pdf",
            checksum: "sha256-v1");

    public static void AddNext(Document document) =>
        document.AddNewVersion(
            originalFileName: "handbook.pdf",
            storedFileName: "v2.pdf",
            contentType: "application/pdf",
            fileSize: 2048,
            storagePath: "documents/x/v2.pdf",
            checksum: "sha256-v2");
}
