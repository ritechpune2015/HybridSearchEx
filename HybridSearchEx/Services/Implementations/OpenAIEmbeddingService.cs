using HybridSearchEx.Models.Embeddings;
using HybridSearchEx.Options;
using HybridSearchEx.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HybridSearchEx.Services.Implementations
{
    public class OpenAIEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAIOptions _options;
        private readonly ILogger<OpenAIEmbeddingService> _logger;

        public OpenAIEmbeddingService(
            HttpClient httpClient,
            IOptions<OpenAIOptions> options,
            ILogger<OpenAIEmbeddingService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<EmbeddingResult> GenerateEmbeddingAsync(
            string text,
            CancellationToken cancellationToken = default)
        {
            // 1. Validate input
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException(
                    "Text cannot be empty.",
                    nameof(text));
            }

            // 2. Create API request
            var requestBody = new OpenAIEmbeddingRequest
            {
                Input = text,
                Model = _options.EmbeddingModel,
                Dimensions = _options.Dimensions,
                EncodingFormat = "float"
            };

            // 3. Create HTTP request
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "embeddings");

            // 4. Add authentication
            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    _options.ApiKey);

            // 5. Add JSON body
            request.Content = JsonContent.Create(requestBody);

            // 6. Send request to OpenAI
            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            // 7. Read response content
            var responseContent =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            // 8. Check API response
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "OpenAI embedding request failed. Status: {StatusCode}",
                    (int)response.StatusCode);

                throw new HttpRequestException(
                    $"OpenAI embedding request failed. " +
                    $"Status: {(int)response.StatusCode}. " +
                    $"Details: {responseContent}",
                    null,
                    response.StatusCode);
            }

            // 9. Deserialize JSON response
            var embeddingResponse =
                JsonSerializer.Deserialize<OpenAIEmbeddingResponse>(
                    responseContent);

            if (embeddingResponse?.Data is null ||
                embeddingResponse.Data.Count == 0)
            {
                throw new InvalidOperationException(
                    "OpenAI returned an empty embedding response.");
            }

            // 10. Extract the first embedding
            var embeddingData = embeddingResponse.Data[0];

            if (embeddingData.Embedding is null ||
                embeddingData.Embedding.Length == 0)
            {
                throw new InvalidOperationException(
                    "OpenAI returned an empty embedding vector.");
            }

            // 11. Validate vector dimensions
            if (embeddingData.Embedding.Length != _options.Dimensions)
            {
                throw new InvalidOperationException(
                    $"Expected {_options.Dimensions} dimensions, " +
                    $"but received {embeddingData.Embedding.Length}.");
            }

            // 12. Convert double[] to float[]
            float[] vector = embeddingData.Embedding
                .Select(value => (float)value)
                .ToArray();

            // 13. Return application model
            return new EmbeddingResult
            {
                Model = embeddingResponse.Model ?? _options.EmbeddingModel,
                Dimensions = vector.Length,
                Vector = vector
            };
        }

        // OpenAI API request model
        private sealed class OpenAIEmbeddingRequest
        {
            [JsonPropertyName("input")]
            public string Input { get; set; } = string.Empty;

            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;

            [JsonPropertyName("dimensions")]
            public int Dimensions { get; set; }

            [JsonPropertyName("encoding_format")]
            public string EncodingFormat { get; set; } = "float";
        }

        // OpenAI API response model
        private sealed class OpenAIEmbeddingResponse
        {
            [JsonPropertyName("data")]
            public List<OpenAIEmbeddingData>? Data { get; set; }

            [JsonPropertyName("model")]
            public string? Model { get; set; }
        }

        private sealed class OpenAIEmbeddingData
        {
            [JsonPropertyName("embedding")]
            public double[]? Embedding { get; set; }

            [JsonPropertyName("index")]
            public int Index { get; set; }
        }
    }
}
