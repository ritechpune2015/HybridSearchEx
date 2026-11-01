using HybridSearchEx.Models.Documents;
using HybridSearchEx.Services.Interfaces;

namespace HybridSearchEx.Services.Implementations
{
    public class DocumentProcessor : IDocumentProcessor
    {
        private readonly IDocumentTextExtractor _textExtractor;
        private readonly ITextChunker _textChunker;
        private readonly ILogger<DocumentProcessor> _logger;

        public DocumentProcessor(
            IDocumentTextExtractor textExtractor,
            ITextChunker textChunker,
            ILogger<DocumentProcessor> logger)
        {
            _textExtractor = textExtractor;
            _textChunker = textChunker;
            _logger = logger;
        }

        public async Task<ProcessedDocument> ProcessAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            if (fileStream is null)
            {
                throw new ArgumentNullException(nameof(fileStream));
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException(
                    "File name is required.",
                    nameof(fileName));
            }

            // Step 1: Generate document ID.
            var documentId = Guid.NewGuid();

            // Step 2: Extract text from the file.
            var extractedText = await _textExtractor.ExtractTextAsync(
                fileStream,
                cancellationToken);

            if (string.IsNullOrWhiteSpace(extractedText))
            {
                throw new InvalidDataException(
                    "The uploaded document does not contain readable text.");
            }

            // Step 3: Create the document model.
            var document = new DocumentModel
            {
                Id = documentId,
                FileName = Path.GetFileName(fileName),
                ContentType = contentType,
                Content = extractedText,
                UploadedAtUtc = DateTime.UtcNow
            };

            // Step 4: Create chunks.
            var chunks = _textChunker.CreateChunks(
                document.Content,
                document.Id,
                document.FileName);

            if (chunks.Count == 0)
            {
                throw new InvalidDataException(
                    "No document chunks could be generated.");
            }

            // Step 5: Log processing details.
            _logger.LogInformation(
                "Document {DocumentName} processed. Characters: {Characters}, Chunks: {Chunks}",
                document.FileName,
                document.Content.Length,
                chunks.Count);

            // Step 6: Return processing result.
            return new ProcessedDocument
            {
                Document = document,
                Chunks = chunks,
                TotalCharacters = document.Content.Length,
                TotalChunks = chunks.Count
            };
        }
    }
}
