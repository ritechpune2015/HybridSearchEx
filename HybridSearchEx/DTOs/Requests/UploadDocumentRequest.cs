using System.ComponentModel.DataAnnotations;

namespace HybridSearchEx.DTOs.Requests
{
    public class UploadDocumentRequest
    {
        [Required]
        public IFormFile File { get; set; } = null!;

    }
}
