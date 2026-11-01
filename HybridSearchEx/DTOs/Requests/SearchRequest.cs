using System.ComponentModel.DataAnnotations;

namespace HybridSearchEx.DTOs.Requests
{
    public class SearchRequest
    {
        [Required]
        [MinLength(3)]
        public string Query { get; set; } = string.Empty;

        [Range(1, 100)]
        public int TopK { get; set; } = 10;

    }
}
