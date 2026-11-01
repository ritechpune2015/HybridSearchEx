namespace HybridSearchEx.Models.Embeddings
{
    public class EmbeddingResult
    {
        public string Model { get; set; } = string.Empty;

        public int Dimensions { get; set; }

        public float[] Vector { get; set; } = [];

    }
}
