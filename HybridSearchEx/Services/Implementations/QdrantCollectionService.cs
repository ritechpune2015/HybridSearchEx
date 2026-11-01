using HybridSearchEx.Options;
using HybridSearchEx.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

namespace HybridSearchEx.Services.Implementations
{
    public class QdrantCollectionService : IQdrantCollectionService
    {
        private readonly HttpClient _httpClient;
        private readonly QdrantOptions _options;
        private readonly ILogger<QdrantCollectionService> _logger;

        public QdrantCollectionService(
            HttpClient httpClient,
            IOptions<QdrantOptions> options,
            ILogger<QdrantCollectionService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<bool> EnsureCollectionExistsAsync(
            CancellationToken cancellationToken = default)
        {
            // 1. Check whether collection already exists.
            using var getRequest = new HttpRequestMessage(
                HttpMethod.Get,
                $"collections/{Uri.EscapeDataString(_options.CollectionName)}");

            using var getResponse = await _httpClient.SendAsync(
                getRequest,
                cancellationToken);

            // 2. If collection exists, don't create it again.
            if (getResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Qdrant collection {CollectionName} already exists.",
                    _options.CollectionName);

                return false;
            }

            // 3. Only create the collection when Qdrant returns 404.
            if (getResponse.StatusCode != HttpStatusCode.NotFound)
            {
                var error = await getResponse.Content.ReadAsStringAsync(
                    cancellationToken);

                throw new HttpRequestException(
                    $"Unable to check Qdrant collection. " +
                    $"Status: {(int)getResponse.StatusCode}. " +
                    $"Details: {error}",
                    null,
                    getResponse.StatusCode);
            }

            // 4. Build the collection configuration.
            var collectionConfiguration = new
            {
                vectors = new Dictionary<string, object>
                {
                    [_options.DenseVectorName] = new
                    {
                        size = _options.DenseVectorDimensions,
                        distance = "Cosine"
                    }
                },

                sparse_vectors = new Dictionary<string, object>
                {
                    [_options.SparseVectorName] = new
                    {
                        modifier = "idf"
                    }
                }
            };

            // 5. Send PUT request to create the collection.
            using var createRequest = new HttpRequestMessage(
                HttpMethod.Put,
                $"collections/{Uri.EscapeDataString(_options.CollectionName)}");

            createRequest.Content = JsonContent.Create(
                collectionConfiguration);

            using var createResponse = await _httpClient.SendAsync(
                createRequest,
                cancellationToken);

            // 6. Check creation response.
            var responseContent =
                await createResponse.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!createResponse.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Qdrant collection creation failed. Status: {StatusCode}. Response: {Response}",
                    (int)createResponse.StatusCode,
                    responseContent);

                throw new HttpRequestException(
                    $"Qdrant collection creation failed. " +
                    $"Status: {(int)createResponse.StatusCode}. " +
                    $"Details: {responseContent}",
                    null,
                    createResponse.StatusCode);
            }

            _logger.LogInformation(
                "Qdrant collection {CollectionName} created successfully.",
                _options.CollectionName);

            return true;
        }

        public async Task<string> GetCollectionInformationAsync(
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"collections/{Uri.EscapeDataString(_options.CollectionName)}");

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            var responseContent =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Unable to retrieve Qdrant collection information. " +
                    $"Status: {(int)response.StatusCode}. " +
                    $"Details: {responseContent}",
                    null,
                    response.StatusCode);
            }

            // Validate that Qdrant returned valid JSON.
            using var jsonDocument = JsonDocument.Parse(responseContent);

            return JsonSerializer.Serialize(
                jsonDocument.RootElement,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });
        }
    }
}
