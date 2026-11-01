using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace HybridSearchEx.Models.Qdrant
{
    public class QdrantPoint
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("vector")]
        public JsonObject Vectors { get; set; } = new();

        [JsonPropertyName("payload")]
        public QdrantPointPayload Payload { get; set; } = new();

    }
}
