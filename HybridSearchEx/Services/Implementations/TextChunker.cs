using HybridSearchEx.Models.Documents;
using HybridSearchEx.Options;
using HybridSearchEx.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace HybridSearchEx.Services.Implementations
{
    public class TextChunker : ITextChunker
    {
        private readonly DocumentProcessingOptions _options;

        public TextChunker(
            IOptions<DocumentProcessingOptions> options)
        {
            _options = options.Value;

            if (_options.ChunkSize <= 0)
            {
                throw new ArgumentException(
                    "Chunk size must be greater than zero.");
            }

            if (_options.ChunkOverlap < 0 ||
                _options.ChunkOverlap >= _options.ChunkSize)
            {
                throw new ArgumentException(
                    "Chunk overlap must be non-negative and smaller than chunk size.");
            }
        }

        public List<DocumentChunk> CreateChunks(
            string text,
            Guid documentId,
            string documentName)
        {
            var chunks = new List<DocumentChunk>();

            if (string.IsNullOrWhiteSpace(text))
            {
                return chunks;
            }

            // Step 1: Normalize the extracted text.
            var normalizedText = NormalizeText(text);

            if (string.IsNullOrWhiteSpace(normalizedText))
            {
                return chunks;
            }

            int start = 0;
            int chunkNumber = 1;

            while (start < normalizedText.Length)
            {
                // Step 2: Calculate the proposed end position.
                int end = Math.Min(
                    start + _options.ChunkSize,
                    normalizedText.Length);

                // Step 3: Try to find a natural boundary.
                if (end < normalizedText.Length)
                {
                    end = FindNaturalBoundary(
                        normalizedText,
                        start,
                        end);
                }

                // Step 4: Extract chunk content.
                var chunkContent = normalizedText[
                    start..end].Trim();

                if (!string.IsNullOrWhiteSpace(chunkContent))
                {
                    chunks.Add(new DocumentChunk
                    {
                        Id = Guid.NewGuid(),

                        DocumentId = documentId,

                        DocumentName = documentName,

                        ChunkNumber = chunkNumber,

                        Content = chunkContent,

                        PageNumber = null,

                        StartIndex = start,

                        EndIndex = end
                    });

                    chunkNumber++;
                }

                // Step 5: Stop when we reach the end.
                if (end >= normalizedText.Length)
                {
                    break;
                }

                // Step 6: Calculate the next starting position.
                int nextStart = end - _options.ChunkOverlap;

                // Safety check to guarantee forward progress.
                if (nextStart <= start)
                {
                    nextStart = end;
                }

                start = nextStart;
            }

            return chunks;
        }

        private static string NormalizeText(string text)
        {
            // Normalize line endings.
            text = text.Replace("\r\n", "\n")
                       .Replace("\r", "\n");

            // Replace tabs with spaces.
            text = text.Replace("\t", " ");

            // Remove unnecessary spaces around lines.
            var lines = text.Split('\n');

            var cleanedLines = lines
                .Select(line => line.Trim())
                .ToList();

            text = string.Join("\n", cleanedLines);

            // Collapse multiple blank lines.
            text = Regex.Replace(
                text,
                @"\n{3,}",
                "\n\n");

            return text.Trim();
        }

        private int FindNaturalBoundary(
            string text,
            int start,
            int proposedEnd)
        {
            int minimumEnd = start +
                (int)(_options.ChunkSize * 0.70);

            // First preference: sentence boundary.
            for (int i = proposedEnd - 1;
                 i >= minimumEnd;
                 i--)
            {
                if (text[i] is '.' or '?' or '!')
                {
                    int boundary = i + 1;

                    // Keep closing punctuation and quotes together.
                    while (boundary < proposedEnd &&
                           text[boundary] is '"' or '\'' or ')' or ']')
                    {
                        boundary++;
                    }

                    if (boundary < text.Length &&
                        char.IsWhiteSpace(text[boundary]))
                    {
                        return boundary;
                    }
                }
            }

            // Second preference: paragraph or line boundary.
            for (int i = proposedEnd - 1;
                 i >= minimumEnd;
                 i--)
            {
                if (text[i] == '\n')
                {
                    return i + 1;
                }
            }

            // Third preference: word boundary.
            for (int i = proposedEnd - 1;
                 i >= minimumEnd;
                 i--)
            {
                if (char.IsWhiteSpace(text[i]))
                {
                    return i;
                }
            }

            // No suitable boundary found.
            return proposedEnd;
        }
    }

}
