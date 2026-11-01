using HybridSearchEx.Models.Search;

namespace HybridSearchEx.Services.Interfaces
{
    public interface IHybridSearchService
    {
        Task<List<SearchResultItem>> SearchAsync(
                string query,
                int topK = 5,
                CancellationToken cancellationToken = default);

    }
}
