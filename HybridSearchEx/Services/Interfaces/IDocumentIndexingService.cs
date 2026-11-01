using HybridSearchEx.Models.Documents;

namespace HybridSearchEx.Services.Interfaces
{
    public interface IDocumentIndexingService
    {
        Task<DocumentIndexingResult> IndexAsync(
    ProcessedDocument processedDocument,
    CancellationToken cancellationToken = default);

    }
}
