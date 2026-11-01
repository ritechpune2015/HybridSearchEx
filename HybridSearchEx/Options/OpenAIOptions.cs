namespace HybridSearchEx.Options
{
    public class OpenAIOptions
    {
        public const string SectionName = "OpenAI";

        public string BaseUrl { get; set; } =
            "https://api.openai.com/v1/";

        public string ApiKey { get; set; } = string.Empty;

        public string EmbeddingModel { get; set; } =
            "text-embedding-3-small";

        public int Dimensions { get; set; } = 1536;

    }
}
