using HybridSearchEx.Models.Qdrant;
using HybridSearchEx.Options;
using HybridSearchEx.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace HybridSearchEx.Services.Implementations
{
    public class QdrantPointStore : IQdrantPointStore
    {
        private readonly HttpClient _httpClient;
        private readonly QdrantOptions _options;
        private readonly ILogger<QdrantPointStore> _logger;

        public QdrantPointStore(
            HttpClient httpClient,
            IOptions<QdrantOptions> options,
            ILogger<QdrantPointStore> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task UpsertPointsAsync(
            List<QdrantPoint> points,
            CancellationToken cancellationToken = default)
        {
            if (points is null || points.Count == 0)
            {
                return;
            }

            var collectionName = Uri.EscapeDataString(
                _options.CollectionName);

            var url = $"collections/{collectionName}/points?wait=true";

            var requestBody = new
            {
                points
            };

            using var response = await _httpClient.PutAsJsonAsync(
                url,
                requestBody,
                cancellationToken);

            var responseContent =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Qdrant upsert failed. Status: {StatusCode}. Response: {Response}",
                    (int)response.StatusCode,
                    responseContent);

                throw new HttpRequestException(
                    $"Qdrant upsert failed. " +
                    $"Status: {(int)response.StatusCode}. " +
                    $"Details: {responseContent}",
                    null,
                    response.StatusCode);
            }

            _logger.LogInformation(
                "Successfully upserted {PointCount} points into {CollectionName}.",
                points.Count,
                _options.CollectionName);
        }
    }
}
