using HybridSearchEx.Models.Documents;
using HybridSearchEx.Models.Qdrant;
using HybridSearchEx.Options;
using HybridSearchEx.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace HybridSearchEx.Services.Implementations
{
    public class DocumentIndexingService : IDocumentIndexingService
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IQdrantPointStore _pointStore;
        private readonly QdrantOptions _qdrantOptions;
        private readonly ILogger<DocumentIndexingService> _logger;

        private const int BatchSize = 16;

        public DocumentIndexingService(
            IEmbeddingService embeddingService,
            IQdrantPointStore pointStore,
            IOptions<QdrantOptions> qdrantOptions,
            ILogger<DocumentIndexingService> logger)
        {
            _embeddingService = embeddingService;
            _pointStore = pointStore;
            _qdrantOptions = qdrantOptions.Value;
            _logger = logger;
        }

        public async Task<DocumentIndexingResult> IndexAsync(
            ProcessedDocument processedDocument,
            CancellationToken cancellationToken = default)
        {
            if (processedDocument is null)
            {
                throw new ArgumentNullException(
                    nameof(processedDocument));
            }

            if (processedDocument.Chunks.Count == 0)
            {
                throw new InvalidOperationException(
                    "No document chunks are available for indexing.");
            }

            var pendingPoints = new List<QdrantPoint>();

            int indexedCount = 0;

            _logger.LogInformation(
                "Starting indexing for document {DocumentName}. Total chunks: {ChunkCount}",
                processedDocument.Document.FileName,
                processedDocument.Chunks.Count);

            // Process each chunk.
            foreach (var chunk in processedDocument.Chunks)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 1. Generate dense embedding using OpenAI.
                var embedding = await _embeddingService
                    .GenerateEmbeddingAsync(
                        chunk.Content,
                        cancellationToken);

                // 2. Validate embedding dimensions.
                if (embedding.Dimensions !=
                    _qdrantOptions.DenseVectorDimensions)
                {
                    throw new InvalidOperationException(
                        $"Embedding dimension mismatch. " +
                        $"Expected {_qdrantOptions.DenseVectorDimensions}, " +
                        $"received {embedding.Dimensions}.");
                }

                // 3. Construct Qdrant point.
                var point = CreateQdrantPoint(
                    chunk,
                    embedding);

                pendingPoints.Add(point);

                // 4. Upsert when batch is full.
                if (pendingPoints.Count >= BatchSize)
                {
                    await _pointStore.UpsertPointsAsync(
                        pendingPoints,
                        cancellationToken);

                    indexedCount += pendingPoints.Count;

                    _logger.LogInformation(
                        "Indexed {IndexedCount}/{TotalCount} chunks.",
                        indexedCount,
                        processedDocument.Chunks.Count);

                    pendingPoints.Clear();
                }
            }

            // 5. Insert remaining points.
            if (pendingPoints.Count > 0)
            {
                await _pointStore.UpsertPointsAsync(
                    pendingPoints,
                    cancellationToken);

                indexedCount += pendingPoints.Count;

                pendingPoints.Clear();
            }

            _logger.LogInformation(
                "Document {DocumentName} indexed successfully. Indexed chunks: {IndexedCount}",
                processedDocument.Document.FileName,
                indexedCount);

            // 6. Return indexing result.
            return new DocumentIndexingResult
            {
                DocumentId = processedDocument.Document.Id,

                DocumentName = processedDocument.Document.FileName,

                TotalChunks = processedDocument.Chunks.Count,

                IndexedChunks = indexedCount,

                IndexedAtUtc = DateTime.UtcNow
            };
        }

        private QdrantPoint CreateQdrantPoint(
            Models.Documents.DocumentChunk chunk,
            Models.Embeddings.EmbeddingResult embedding)
        {
            var vectors = new System.Text.Json.Nodes.JsonObject
            {
                [_qdrantOptions.DenseVectorName] =
                    System.Text.Json.JsonSerializer.SerializeToNode(
                        embedding.Vector),

                [_qdrantOptions.SparseVectorName] =
                    new System.Text.Json.Nodes.JsonObject
                    {
                        ["text"] = chunk.Content,
                        ["model"] = "qdrant/bm25"
                    }
            };

            return new QdrantPoint
            {
                Id = chunk.Id,

                Vectors = vectors,

                Payload = new QdrantPointPayload
                {
                    DocumentId = chunk.DocumentId.ToString(),

                    DocumentName = chunk.DocumentName,

                    ChunkNumber = chunk.ChunkNumber,

                    Content = chunk.Content,

                    PageNumber = chunk.PageNumber,

                    StartIndex = chunk.StartIndex,

                    EndIndex = chunk.EndIndex,

                    Length = chunk.Length
                }
            };
        }

    }
}