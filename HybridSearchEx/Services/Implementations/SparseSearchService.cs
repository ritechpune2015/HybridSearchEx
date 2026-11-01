using HybridSearchEx.Models.Search;
using HybridSearchEx.Services.Interfaces;

namespace HybridSearchEx.Services.Implementations
{
    public class SparseSearchService : ISparseSearchService
    {
        private readonly IQdrantSearchStore _qdrantSearchStore;
        private readonly ILogger<SparseSearchService> _logger;

        public SparseSearchService(
            IQdrantSearchStore qdrantSearchStore,
            ILogger<SparseSearchService> logger)
        {
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
                "Starting sparse search for query: {Query}",
                query);

            var results =
                await _qdrantSearchStore.SearchSparseAsync(
                    query,
                    topK,
                    cancellationToken);

            _logger.LogInformation(
                "Sparse search completed. Results found: {Count}",
                results.Count);

            return results;
        }
    }
}
