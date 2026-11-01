namespace HybridSearchEx.DTOs.Responses
{
    public class SearchResponse
    {
        public string Query { get; set; } = string.Empty;

        public string SearchType { get; set; } = string.Empty;

        public int TotalResults { get; set; }

        public long ExecutionTimeMs { get; set; }

        public List<SearchResultDto> Results { get; set; } = [];

    }
}
