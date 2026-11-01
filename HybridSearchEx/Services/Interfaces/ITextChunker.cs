using HybridSearchEx.Models.Documents;

namespace HybridSearchEx.Services.Interfaces
{
    public interface ITextChunker
    {
        List<DocumentChunk> CreateChunks(string text, Guid documentId, string documentName);

    }
}
