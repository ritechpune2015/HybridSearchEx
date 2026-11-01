namespace HybridSearchEx.Options
{
    public class DocumentProcessingOptions
    {
        public const string SectionName = "DocumentProcessing";

        public int ChunkSize { get; set; } = 1200;

        public int ChunkOverlap { get; set; } = 200;

        public long MaxFileSizeBytes { get; set; } =
            10 * 1024 * 1024;

        public string[] AllowedExtensions { get; set; } =
        [
            ".txt"
        ];

    }
}
