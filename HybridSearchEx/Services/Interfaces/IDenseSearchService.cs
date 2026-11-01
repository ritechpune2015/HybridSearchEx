using HybridSearchEx.Models.Search;

namespace HybridSearchEx.Services.Interfaces
{
    public interface IDenseSearchService
    {
        Task<List<SearchResultItem>> SearchAsync(
        string query,
        int topK = 5,
        CancellationToken cancellationToken = default);

    }
}
