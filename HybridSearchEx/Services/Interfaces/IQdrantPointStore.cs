using HybridSearchEx.Models.Qdrant;

namespace HybridSearchEx.Services.Interfaces
{
    public interface IQdrantPointStore
    {
        Task UpsertPointsAsync(
    List<QdrantPoint> points,
    CancellationToken cancellationToken = default);

    }
}
