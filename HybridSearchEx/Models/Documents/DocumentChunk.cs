namespace HybridSearchEx.Models.Documents
{
    public class DocumentChunk
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid DocumentId { get; set; }

        public string DocumentName { get; set; } = string.Empty;

        public int ChunkNumber { get; set; }

        public string Content { get; set; } = string.Empty;

        public int? PageNumber { get; set; }

        public int StartIndex { get; set; }

        public int EndIndex { get; set; }

        public int Length => Content.Length;

    }
}
