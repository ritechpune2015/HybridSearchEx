using System.Text.Json.Serialization;

namespace HybridSearchEx.Models.Qdrant
{
    public class QdrantInferenceVector
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("model")]
        public string Model { get; set; } = "Qdrant/bm25";

    }
}
