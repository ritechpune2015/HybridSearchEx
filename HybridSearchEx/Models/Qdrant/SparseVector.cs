namespace HybridSearchEx.Models.Qdrant
{
    public class SparseVector
    {
        public List<uint> Indices { get; set; } = [];
        public List<float> Values { get; set; } = [];

    }
}
