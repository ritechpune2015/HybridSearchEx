namespace HybridSearchEx.DTOs.Responses
{
    public class UploadDocumentResponse
    {
        public Guid DocumentId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public int TotalCharacters { get; set; }

        public int ChunkCount { get; set; }

        public List<ChunkPreviewDto> Chunks { get; set; } = [];

        public string Message { get; set; } = string.Empty;
    }
}
