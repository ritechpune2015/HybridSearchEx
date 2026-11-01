namespace HybridSearchEx.Models.Documents
{
    public class DocumentModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;

    }
}
