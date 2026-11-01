namespace HybridSearchEx.Models.Documents
{
    public class DocumentIndexingResponse
    {
        public Guid DocumentId { get; set; }

        public string DocumentName { get; set; } = string.Empty;

        public int TotalChunks { get; set; }

        public int IndexedChunks { get; set; }

        public string Message { get; set; } = string.Empty;

    }
}
