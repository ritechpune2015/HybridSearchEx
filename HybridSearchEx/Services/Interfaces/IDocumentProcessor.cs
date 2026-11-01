using HybridSearchEx.Models.Documents;

namespace HybridSearchEx.Services.Interfaces
{
    public interface IDocumentProcessor
    {
        Task<ProcessedDocument> ProcessAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default);
    }

}
