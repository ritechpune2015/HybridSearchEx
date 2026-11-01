using HybridSearchEx.Models.Search;
using HybridSearchEx.Options;
using HybridSearchEx.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using System.Drawing;
using System.Text.Json;

namespace HybridSearchEx.Services.Implementations
{
    public class QdrantSearchStore : IQdrantSearchStore
    {
        private readonly HttpClient _httpClient;
        private readonly QdrantOptions _options;
        private readonly ILogger<QdrantSearchStore> _logger;

        public QdrantSearchStore(
            HttpClient httpClient,
            IOptions<QdrantOptions> options,
            ILogger<QdrantSearchStore> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<List<SearchResultItem>> SearchDenseAsync(
            float[] queryVector,
            int limit,
            CancellationToken cancellationToken = default)
        {
            if (queryVector is null || queryVector.Length == 0)
            {
                throw new ArgumentException(
                    "Query vector cannot be empty.",
                    nameof(queryVector));
            }

            if (queryVector.Length != _options.DenseVectorDimensions)
            {
                throw new ArgumentException(
                    $"Expected {_options.DenseVectorDimensions} dimensions, " +
                    $"but received {queryVector.Length}.");
            }

            var collectionName =
                Uri.EscapeDataString(_options.CollectionName);

            var url = $"collections/{collectionName}/points/query";

            var requestBody = new
            {
                query = queryVector,

                @using = _options.DenseVectorName,

                limit = limit,

                with_payload = true,

                with_vector = false
            };

            var httpResponse = await _httpClient.PostAsJsonAsync(
            url,
            requestBody,
            cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var error = await httpResponse.Content.ReadAsStringAsync(
                    cancellationToken);

                _logger.LogError(
                                "Qdrant dense search failed. Status: {Status}. " +
                            "Response: {Response}",
                            httpResponse.StatusCode,
                            error);

                throw new HttpRequestException(
                    $"Qdrant search failed: {error}",
                    null,
                    httpResponse.StatusCode);
            }

            await using var responseStream =
                        await httpResponse.Content.ReadAsStreamAsync(
                            cancellationToken);

            using var jsonDocument =
                await JsonDocument.ParseAsync(
                    responseStream,
                    cancellationToken: cancellationToken);

            var results = new List<SearchResultItem>();

            var points = jsonDocument.RootElement
                .GetProperty("result")
                .GetProperty("points");

            foreach (var point in points.EnumerateArray())
            {
                var payload = point.GetProperty("payload");

                var item = new SearchResultItem
                {
                    PointId = point.GetProperty("id").GetGuid(),

                    Score = point.GetProperty("score").GetDouble(),

                    DocumentId = payload
                        .GetProperty("documentId")
                        .GetGuid().ToString(),

                    DocumentName = payload
                        .GetProperty("documentName")
                        .GetString() ?? string.Empty,

                    ChunkNumber = payload
                        .GetProperty("chunkNumber")
                        .GetInt32(),

                    Content = payload
                        .GetProperty("content")
                        .GetString() ?? string.Empty,

                    StartIndex = payload
                        .GetProperty("startIndex")
                        .GetInt32(),

                    EndIndex = payload
                        .GetProperty("endIndex")
                        .GetInt32()
                };

                results.Add(item);
            }

            return results;
        }
    
    public async Task<List<SearchResultItem>> SearchSparseAsync(
    string queryText,
    int limit,
    CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(queryText))
            {
                throw new ArgumentException(
                    "Search query cannot be empty.",
                    nameof(queryText));
            }

            if (limit < 1 || limit > 50)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(limit),
                    "Limit must be between 1 and 50.");
            }

            var collectionName =
                Uri.EscapeDataString(_options.CollectionName);

            var url =
                $"collections/{collectionName}/points/query";

            // Qdrant BM25 inference request.
            var bm25Query = new Dictionary<string, object>
            {
                ["text"] = queryText,
                ["model"] = "qdrant/bm25"
            };

            // Build the Qdrant query request.
            var requestBody = new Dictionary<string, object>
            {
                ["query"] = bm25Query,
                ["using"] = _options.SparseVectorName,
                ["limit"] = limit,
                ["with_payload"] = true,
                ["with_vector"] = false
            };

            var httpResponse = await _httpClient.PostAsJsonAsync(
                url,
                requestBody,
                cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var error =
                    await httpResponse.Content.ReadAsStringAsync(
                        cancellationToken);

                _logger.LogError(
                    "Qdrant sparse search failed. Status: {Status}. " +
                    "Response: {Response}",
                    httpResponse.StatusCode,
                    error);

                throw new HttpRequestException(
                    $"Qdrant sparse search failed: {error}",
                    null,
                    httpResponse.StatusCode);
            }

            await using var responseStream =
                await httpResponse.Content.ReadAsStreamAsync(
                    cancellationToken);

            using var jsonDocument =
                await JsonDocument.ParseAsync(
                    responseStream,
                    cancellationToken: cancellationToken);

            var results = new List<SearchResultItem>();

            var points = jsonDocument.RootElement
                .GetProperty("result")
                .GetProperty("points");

            foreach (var point in points.EnumerateArray())
            {
                var payload = point.GetProperty("payload");

                var item = new SearchResultItem
                {
                    PointId = point.GetProperty("id").GetGuid(),

                    Score = point.GetProperty("score").GetDouble(),

                    DocumentId = payload
                        .GetProperty("documentId")
                        .GetGuid().ToString(),

                    DocumentName = payload
                        .GetProperty("documentName")
                        .GetString() ?? string.Empty,

                    ChunkNumber = payload
                        .GetProperty("chunkNumber")
                        .GetInt32(),

                    Content = payload
                        .GetProperty("content")
                        .GetString() ?? string.Empty,

                    StartIndex = payload
                        .GetProperty("startIndex")
                        .GetInt32(),

                    EndIndex = payload
                        .GetProperty("endIndex")
                        .GetInt32()
                };

                results.Add(item);
            }

            return results;
        }

        public async Task<List<SearchResultItem>> SearchHybridAsync(
    float[] queryVector,
    string queryText,
    int limit,
    CancellationToken cancellationToken = default)
        {
            if (queryVector is null || queryVector.Length == 0)
            {
                throw new ArgumentException(
                    "Query vector cannot be empty.",
                    nameof(queryVector));
            }

            if (queryVector.Length != _options.DenseVectorDimensions)
            {
                throw new ArgumentException(
                    $"Expected {_options.DenseVectorDimensions} dimensions, " +
                    $"but received {queryVector.Length}.");
            }

            if (string.IsNullOrWhiteSpace(queryText))
            {
                throw new ArgumentException(
                    "Search query cannot be empty.",
                    nameof(queryText));
            }

            if (limit < 1 || limit > 50)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(limit),
                    "Limit must be between 1 and 50.");
            }

            var collectionName =
                Uri.EscapeDataString(_options.CollectionName);

            var url =
                $"collections/{collectionName}/points/query";

            // Retrieve more candidates before final RRF fusion.
            var prefetchLimit = Math.Max(limit * 4, 20);

            // Dense retrieval request.
            var densePrefetch = new Dictionary<string, object>
            {
                ["query"] = queryVector,
                ["using"] = _options.DenseVectorName,
                ["limit"] = prefetchLimit
            };

            // Sparse BM25 retrieval request.
            var sparseQuery = new Dictionary<string, object>
            {
                ["text"] = queryText,
                ["model"] = "qdrant/bm25"
            };

            var sparsePrefetch = new Dictionary<string, object>
            {
                ["query"] = sparseQuery,
                ["using"] = _options.SparseVectorName,
                ["limit"] = prefetchLimit
            };

            // Combine both retrieval operations using RRF.
            var rrfQuery = new Dictionary<string, object>
            {
                ["rrf"] = new Dictionary<string, object>()
            };

            var requestBody = new Dictionary<string, object>
            {
                ["prefetch"] = new object[]
                {
            densePrefetch,
            sparsePrefetch
                },

                ["query"] = rrfQuery,

                ["limit"] = limit,

                ["with_payload"] = true,

                ["with_vector"] = false
            };

            var httpResponse = await _httpClient.PostAsJsonAsync(
                url,
                requestBody,
                cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var error =
                    await httpResponse.Content.ReadAsStringAsync(
                        cancellationToken);

                _logger.LogError(
                    "Qdrant hybrid search failed. Status: {Status}. " +
                    "Response: {Response}",
                    httpResponse.StatusCode,
                    error);

                throw new HttpRequestException(
                    $"Qdrant hybrid search failed: {error}",
                    null,
                    httpResponse.StatusCode);
            }

            await using var responseStream =
                await httpResponse.Content.ReadAsStreamAsync(
                    cancellationToken);

            using var jsonDocument =
                await JsonDocument.ParseAsync(
                    responseStream,
                    cancellationToken: cancellationToken);

            var results = new List<SearchResultItem>();

            var points = jsonDocument.RootElement
                .GetProperty("result")
                .GetProperty("points");

            foreach (var point in points.EnumerateArray())
            {
                var payload = point.GetProperty("payload");

                var item = new SearchResultItem
                {
                    PointId = point.GetProperty("id").GetGuid(),

                    Score = point.GetProperty("score").GetDouble(),

                    DocumentId = payload
                        .GetProperty("documentId")
                        .GetGuid().ToString(),

                    DocumentName = payload
                        .GetProperty("documentName")
                        .GetString() ?? string.Empty,

                    ChunkNumber = payload
                        .GetProperty("chunkNumber")
                        .GetInt32(),

                    Content = payload
                        .GetProperty("content")
                        .GetString() ?? string.Empty,

                    StartIndex = payload
                        .GetProperty("startIndex")
                        .GetInt32(),

                    EndIndex = payload
                        .GetProperty("endIndex")
                        .GetInt32()
                };

                results.Add(item);
            }

            return results;
        }

    }
}
