using Acme.DocumentManagement.Domain.Aggregates.Documents;
using Acme.SharedKernel.Exceptions;

namespace Acme.DocumentManagement.Application.Commands.ArchiveDocument;

public sealed class ArchiveDocumentHandler
{
    private readonly IDocumentRepository _repository;

    public ArchiveDocumentHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        ArchiveDocumentCommand command,
        CancellationToken cancellationToken)
    {
        var document = await _repository.GetByIdAsync(
            command.DocumentId,
            cancellationToken);

        if (document is null)
            throw new NotFoundException(
                DocumentLifecycleErrors.DocumentDoesNotExist);

        document.Archive();

        await _repository.UpdateAsync(document, cancellationToken);
    }
}
