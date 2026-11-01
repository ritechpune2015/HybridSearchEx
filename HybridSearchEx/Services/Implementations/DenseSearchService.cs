using HybridSearchEx.Models.Search;
using HybridSearchEx.Services.Interfaces;

namespace HybridSearchEx.Services.Implementations
{
    public class DenseSearchService : IDenseSearchService
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IQdrantSearchStore _qdrantSearchStore;
        private readonly ILogger<DenseSearchService> _logger;

        public DenseSearchService(
            IEmbeddingService embeddingService,
            IQdrantSearchStore qdrantSearchStore,
            ILogger<DenseSearchService> logger)
        {
            _embeddingService = embeddingService;
            _qdrantSearchStore = qdrantSearchStore;
            _logger = logger;
        }

        public async Task<List<SearchResultItem>> SearchAsync(
            string query,
            int topK = 5,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException(
                    "Search query cannot be empty.",
                    nameof(query));
            }

            if (topK < 1 || topK > 50)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(topK),
                    "TopK must be between 1 and 50.");
            }

            _logger.LogInformation(
                "Starting dense search for query: {Query}",
                query);

            // Step 1: Convert the user's question into an embedding.
            var embedding =
                await _embeddingService.GenerateEmbeddingAsync(
                    query,
                    cancellationToken);

            // Step 2: Search Qdrant using the dense vector.
            var results =
                await _qdrantSearchStore.SearchDenseAsync(
                    embedding.Vector,
                    topK,
                    cancellationToken);

            _logger.LogInformation(
                "Dense search completed. Results found: {Count}",
                results.Count);

            return results;
        }
    }
}
