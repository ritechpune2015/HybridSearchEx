using System.ComponentModel.DataAnnotations;

namespace HybridSearchEx.DTOs.Requests
{
    public class GenerateEmbeddingRequest
    {
        [Required]
        [MinLength(1)]
        public string Text { get; set; } = string.Empty;

    }
}
