namespace HybridSearchEx.DTOs.Responses
{
    public class ChunkPreviewDto
    {
        public int ChunkNumber { get; set; }

        public int StartIndex { get; set; }

        public int EndIndex { get; set; }

        public int Length { get; set; }

        public string Content { get; set; } = string.Empty;
    }
}
