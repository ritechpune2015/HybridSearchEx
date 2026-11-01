using HybridSearchEx.DTOs.Requests;
using HybridSearchEx.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HybridSearchEx.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmbeddingsController : ControllerBase
    {
        private readonly IEmbeddingService _embeddingService;

        public EmbeddingsController(
            IEmbeddingService embeddingService)
        {
            _embeddingService = embeddingService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate(
            [FromBody] GenerateEmbeddingRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _embeddingService.GenerateEmbeddingAsync(
                request.Text,
                cancellationToken);

            return Ok(new
            {
                Model = result.Model,
                Dimensions = result.Dimensions,

                // Return only the first 10 values for testing.
                SampleValues = result.Vector.Take(10),

                TotalValues = result.Vector.Length
            });
        }
    }
}
