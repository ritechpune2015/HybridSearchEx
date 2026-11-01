namespace HybridSearchEx.Models.Qdrant
{
    public class QdrantPointPayload
    {
        public string DocumentId { get; set; } = string.Empty;

        public string DocumentName { get; set; } = string.Empty;

        public int ChunkNumber { get; set; }

        public string Content { get; set; } = string.Empty;

        public int? PageNumber { get; set; }

        public int StartIndex { get; set; }

        public int EndIndex { get; set; }

        public int Length { get; set; }

    }
}
