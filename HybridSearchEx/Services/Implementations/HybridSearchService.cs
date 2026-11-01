using HybridSearchEx.Models.Search;
using HybridSearchEx.Services.Interfaces;

namespace HybridSearchEx.Services.Implementations
{
    public class HybridSearchService : IHybridSearchService
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IQdrantSearchStore _qdrantSearchStore;
        private readonly ILogger<HybridSearchService> _logger;

        public HybridSearchService(
            IEmbeddingService embeddingService,
            IQdrantSearchStore qdrantSearchStore,
            ILogger<HybridSearchService> logger)
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
                "Starting hybrid search for query: {Query}",
                query);

            // Step 1: Generate dense embedding for the user query.
            var embedding =
                await _embeddingService.GenerateEmbeddingAsync(
                    query,
                    cancellationToken);

            // Step 2: Execute dense + sparse retrieval and RRF fusion.
            var results =
                await _qdrantSearchStore.SearchHybridAsync(
                    embedding.Vector,
                    query,
                    topK,
                    cancellationToken);

            _logger.LogInformation(
                "Hybrid search completed. Results found: {Count}",
                results.Count);

            return results;
        }
    }
}

