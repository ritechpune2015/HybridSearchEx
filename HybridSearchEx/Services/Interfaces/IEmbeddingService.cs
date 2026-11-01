using HybridSearchEx.Models.Embeddings;

namespace HybridSearchEx.Services.Interfaces
{
    public interface IEmbeddingService
    {
        Task<EmbeddingResult> GenerateEmbeddingAsync(
            string text,
            CancellationToken cancellationToken = default);

    }
}
