namespace HybridSearchEx.Models.Documents
{
    public class ProcessedDocument
    {
        public DocumentModel Document { get; set; } = new();

        public List<DocumentChunk> Chunks { get; set; } = [];

        public int TotalCharacters { get; set; }

        public int TotalChunks { get; set; }

    }
}
