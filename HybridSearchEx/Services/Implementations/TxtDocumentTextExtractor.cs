using HybridSearchEx.Services.Interfaces;
using System.Text;

namespace HybridSearchEx.Services.Implementations
{
    public class TxtDocumentTextExtractor : IDocumentTextExtractor
    {
        public async Task<string> ExtractTextAsync(
            Stream fileStream,
            CancellationToken cancellationToken = default)
        {
            if (fileStream is null)
            {
                throw new ArgumentNullException(nameof(fileStream));
            }

            using var memoryStream = new MemoryStream();

            await fileStream.CopyToAsync(
                memoryStream,
                cancellationToken);

            var bytes = memoryStream.ToArray();

            // Detect UTF-8 BOM, UTF-16 BOM and UTF-32 BOM.
            Encoding.RegisterProvider(
                CodePagesEncodingProvider.Instance);

            Encoding encoding;

            if (bytes.Length >= 4 &&
                bytes[0] == 0xFF &&
                bytes[1] == 0xFE &&
                bytes[2] == 0x00 &&
                bytes[3] == 0x00)
            {
                encoding = new UTF32Encoding(
                    bigEndian: false,
                    byteOrderMark: true,
                    throwOnInvalidCharacters: true);
            }
            else if (bytes.Length >= 4 &&
                     bytes[0] == 0x00 &&
                     bytes[1] == 0x00 &&
                     bytes[2] == 0xFE &&
                     bytes[3] == 0xFF)
            {
                encoding = new UTF32Encoding(
                    bigEndian: true,
                    byteOrderMark: true,
                    throwOnInvalidCharacters: true);
            }
            else if (bytes.Length >= 2 &&
                     bytes[0] == 0xFF &&
                     bytes[1] == 0xFE)
            {
                encoding = new UnicodeEncoding(
                    bigEndian: false,
                    byteOrderMark: true,
                    throwOnInvalidBytes: true);
            }
            else if (bytes.Length >= 2 &&
                     bytes[0] == 0xFE &&
                     bytes[1] == 0xFF)
            {
                encoding = new UnicodeEncoding(
                    bigEndian: true,
                    byteOrderMark: true,
                    throwOnInvalidBytes: true);
            }
            else
            {
                encoding = new UTF8Encoding(
                    encoderShouldEmitUTF8Identifier: false,
                    throwOnInvalidBytes: true);
            }

            try
            {
                return encoding.GetString(bytes);
            }
            catch (DecoderFallbackException ex)
            {
                throw new InvalidDataException(
                    "The uploaded text file contains invalid or unsupported text encoding.",
                    ex);
            }
        }
    }
}
