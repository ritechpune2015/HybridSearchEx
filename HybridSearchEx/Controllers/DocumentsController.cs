using HybridSearchEx.DTOs.Responses;
using HybridSearchEx.Models.Documents;
using HybridSearchEx.Options;
using HybridSearchEx.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HybridSearchEx.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentProcessor _documentProcessor;
        private readonly DocumentProcessingOptions _options;
        private readonly IDocumentIndexingService _indexingService;


        public DocumentsController(
             IDocumentProcessor documentProcessor,
             IDocumentIndexingService indexingService,
             IOptions<DocumentProcessingOptions> options)
        {
            _documentProcessor = documentProcessor;
            _indexingService = indexingService;
            _options = options.Value;
        }


        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> Upload(
             IFormFile file,
             CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
            {
                return BadRequest(new
                {
                    Message = "Please upload a non-empty file."
                });
            }

            if (file.Length > _options.MaxFileSizeBytes)
            {
                return BadRequest(new
                {
                    Message = "Uploaded file exceeds the maximum allowed size."
                });
            }

            var extension = Path.GetExtension(file.FileName);

            if (!_options.AllowedExtensions.Contains(
                    extension,
                    StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    Message = "Only TXT files are supported."
                });
            }

            try
            {
                // Step 1: Process the uploaded document.
                await using var fileStream = file.OpenReadStream();

                var processedDocument =
                    await _documentProcessor.ProcessAsync(
                        fileStream,
                        file.FileName,
                        file.ContentType,
                        cancellationToken);

                // Step 2: Index the generated chunks.
                var indexingResult =
                    await _indexingService.IndexAsync(
                        processedDocument,
                        cancellationToken);

                // Step 3: Return indexing response.
                var response = new DocumentIndexingResponse
                {
                    DocumentId = indexingResult.DocumentId,

                    DocumentName = indexingResult.DocumentName,

                    TotalChunks = indexingResult.TotalChunks,

                    IndexedChunks = indexingResult.IndexedChunks,

                    Message = "Document processed and indexed successfully."
                };

                return Ok(response);
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message
                });
            }
            catch (HttpRequestException ex)
            {
                // Log the exception; avoid exposing API credentials
                // or internal infrastructure details to the client.
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    new
                    {
                        Message = "Document indexing failed because an external service returned an error."
                    });
            }
        }
    }
}
