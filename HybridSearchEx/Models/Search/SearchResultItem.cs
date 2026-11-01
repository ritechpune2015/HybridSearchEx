namespace HybridSearchEx.Models.Search
{
    public class SearchResultItem
    {
        public Guid PointId { get; set; }

        public string DocumentId { get; set; } = string.Empty;

        public string DocumentName { get; set; } = string.Empty;

        public int ChunkNumber { get; set; }

        public string Content { get; set; } = string.Empty;

        public int? PageNumber { get; set; }

        public double Score { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    }

}
