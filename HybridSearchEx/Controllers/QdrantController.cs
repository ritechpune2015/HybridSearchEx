using HybridSearchEx.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HybridSearchEx.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QdrantController : ControllerBase
    {
        private readonly IQdrantCollectionService _collectionService;

        public QdrantController(
            IQdrantCollectionService collectionService)
        {
            _collectionService = collectionService;
        }

        [HttpPost("create-collection")]
        public async Task<IActionResult> CreateCollection(
            CancellationToken cancellationToken)
        {
            var created =
                await _collectionService.EnsureCollectionExistsAsync(
                    cancellationToken);

            return Ok(new
            {
                CollectionName = "hybrid_documents",

                Created = created,

                Message = created
                    ? "Qdrant hybrid search collection created successfully."
                    : "Qdrant collection already exists."
            });
        }

        [HttpGet("collection-info")]
        public async Task<IActionResult> GetCollectionInfo(
            CancellationToken cancellationToken)
        {
            var information =
                await _collectionService.GetCollectionInformationAsync(
                    cancellationToken);

            return Content(
                information,
                "application/json");
        }
    }
}
