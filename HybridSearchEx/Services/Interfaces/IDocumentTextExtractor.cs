namespace HybridSearchEx.Services.Interfaces
{
    public interface IDocumentTextExtractor
    {
        Task<string> ExtractTextAsync(Stream fileStream, CancellationToken cancellationToken = default);

    }
}
