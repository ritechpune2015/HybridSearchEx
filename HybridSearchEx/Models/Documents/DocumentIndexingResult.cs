namespace HybridSearchEx.Models.Documents
{
    public class DocumentIndexingResult
    {
        public Guid DocumentId { get; set; }

        public string DocumentName { get; set; } = string.Empty;

        public int TotalChunks { get; set; }

        public int IndexedChunks { get; set; }

        public DateTime IndexedAtUtc { get; set; } = DateTime.UtcNow;

    }
}
