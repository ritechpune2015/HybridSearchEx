namespace HybridSearchEx.Services.Interfaces
{
    public interface IQdrantCollectionService
    {
        Task<bool> EnsureCollectionExistsAsync(
    CancellationToken cancellationToken = default);

        Task<string> GetCollectionInformationAsync(
            CancellationToken cancellationToken = default);

    }
}
