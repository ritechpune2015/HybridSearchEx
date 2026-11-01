using HybridSearchEx.DTOs.Requests;
using HybridSearchEx.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HybridSearchEx.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly IDenseSearchService _denseSearchService;
        private readonly ISparseSearchService _sparseSearchService;

        private readonly IHybridSearchService _hybridSearchService;

        public SearchController(
    IDenseSearchService denseSearchService,
    ISparseSearchService sparseSearchService,
    IHybridSearchService hybridSearchService)
        {
            _denseSearchService = denseSearchService;
            _sparseSearchService = sparseSearchService;
            _hybridSearchService = hybridSearchService;
        }



        [HttpPost("dense")]
        public async Task<IActionResult> DenseSearch(
            [FromBody] SearchRequest request,
            CancellationToken cancellationToken)
        {
            if (request is null ||
                string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(new
                {
                    Message = "Please provide a search query."
                });
            }

            var topK = request.TopK <= 0
                ? 5
                : request.TopK;

            if (topK > 50)
            {
                return BadRequest(new
                {
                    Message = "TopK cannot exceed 50."
                });
            }

            try
            {
                var results =
                    await _denseSearchService.SearchAsync(
                        request.Query,
                        topK,
                        cancellationToken);

                return Ok(new
                {
                    Query = request.Query,
                    SearchType = "Dense Vector Search",
                    TotalResults = results.Count,
                    Results = results
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message
                });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new
                {
                    Message = "An external search service failed.",
                    Details = ex.Message
                });
            }
        }

        [HttpPost("sparse")]
        public async Task<IActionResult> SparseSearch(
            [FromBody] SearchRequest request,
            CancellationToken cancellationToken)
        {
            if (request is null ||
                string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(new
                {
                    Message = "Please provide a search query."
                });
            }

            var topK = request.TopK <= 0 ? 5 : request.TopK;

            if (topK > 50)
            {
                return BadRequest(new
                {
                    Message = "TopK cannot exceed 50."
                });
            }

            try
            {
                var results = await _sparseSearchService.SearchAsync(
                    request.Query,
                    topK,
                    cancellationToken);

                return Ok(new
                {
                    Query = request.Query,
                    SearchType = "BM25 Sparse Vector Search",
                    TotalResults = results.Count,
                    Results = results
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message
                });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new
                {
                    Message = "An external search service failed.",
                    Details = ex.Message
                });
            }
        }
        [HttpPost("hybrid")]
        public async Task<IActionResult> HybridSearch(
            [FromBody] SearchRequest request,
            CancellationToken cancellationToken)
        {
            if (request is null ||
                string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(new
                {
                    Message = "Please provide a search query."
                });
            }

            var topK = request.TopK <= 0 ? 5 : request.TopK;

            if (topK > 50)
            {
                return BadRequest(new
                {
                    Message = "TopK cannot exceed 50."
                });
            }

            try
            {
                var results = await _hybridSearchService.SearchAsync(
                    request.Query,
                    topK,
                    cancellationToken);

                return Ok(new
                {
                    Query = request.Query,
                    SearchType = "Hybrid Search (Dense + BM25 + RRF)",
                    TotalResults = results.Count,
                    Results = results
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message
                });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new
                {
                    Message = "An external search service failed.",
                    Details = ex.Message
                });
            }
        }
    }
}