namespace HybridSearchEx.Options
{
    public class QdrantOptions
    {
        public const string SectionName = "Qdrant";

        public string BaseUrl { get; set; } ="http://localhost:6333";

        public string? ApiKey { get; set; }

        public string CollectionName { get; set; } ="hybrid_documents";

        public string DenseVectorName { get; set; } ="dense";

        public string SparseVectorName { get; set; } ="bm25";

        public int DenseVectorDimensions { get; set; } = 1536;

    }
}
