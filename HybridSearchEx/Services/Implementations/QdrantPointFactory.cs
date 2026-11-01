using HybridSearchEx.Models.Documents;
using HybridSearchEx.Models.Embeddings;
using HybridSearchEx.Models.Qdrant;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HybridSearchEx.Services.Implementations
{
    public class QdrantPointFactory
    {
        public static QdrantPoint Create(
        DocumentChunk chunk,
        EmbeddingResult embedding)
        {
            var vectors = new JsonObject
            {
                ["dense"] = JsonSerializer.SerializeToNode(
                    embedding.Vector),

                ["bm25"] = new JsonObject
                {
                    ["text"] = chunk.Content,
                    ["model"] = "qdrant/bm25"
                }
            };

            var payload = new QdrantPointPayload
            {
                DocumentId = chunk.DocumentId.ToString(),

                DocumentName = chunk.DocumentName,

                ChunkNumber = chunk.ChunkNumber,

                Content = chunk.Content,

                PageNumber = chunk.PageNumber,

                StartIndex = chunk.StartIndex,

                EndIndex = chunk.EndIndex,

                Length = chunk.Length
            };

            return new QdrantPoint
            {
                Id = chunk.Id,

                Vectors = vectors,

                Payload = payload
            };
        }
    }

}

