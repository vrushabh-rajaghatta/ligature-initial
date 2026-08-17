using Acme.SharedKernel.Primitives;

namespace Acme.DocumentManagement.Domain.Aggregates.Documents;

public sealed class DocumentVersionId : StronglyTypedId
{
    public DocumentVersionId(Guid value) : base(value)
    {
    }

    public static DocumentVersionId New() => new(Guid.NewGuid());

    public static DocumentVersionId From(Guid value) => new(value);

    public static implicit operator Guid(DocumentVersionId id) => id.Value;
}
