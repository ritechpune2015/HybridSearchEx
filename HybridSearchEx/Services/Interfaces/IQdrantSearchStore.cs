using HybridSearchEx.Models.Search;

namespace HybridSearchEx.Services.Interfaces
{
    public interface IQdrantSearchStore
    {
        Task<List<SearchResultItem>> SearchDenseAsync(
        float[] queryVector,
        int limit,
        CancellationToken cancellationToken = default);

        Task<List<SearchResultItem>> SearchSparseAsync(
          string queryText,
          int limit,
          CancellationToken cancellationToken = default);

        Task<List<SearchResultItem>> SearchHybridAsync(
    float[] queryVector,
    string queryText,
    int limit,
    CancellationToken cancellationToken = default);

    }
}
