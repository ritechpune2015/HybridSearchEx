using HybridSearchEx.Options;
using HybridSearchEx.Services.Implementations;
using HybridSearchEx.Services.Interfaces;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Register MVC controllers
builder.Services.AddControllers();

// Register OpenAPI
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

// Read OpenAI configuration
builder.Services
    .AddOptions<OpenAIOptions>()
    .Bind(builder.Configuration.GetSection(OpenAIOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ApiKey),
        "OpenAI API key is missing.")
    .Validate(
        options => Uri.TryCreate(
            options.BaseUrl,
            UriKind.Absolute,
            out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttps ||
             uri.Scheme == Uri.UriSchemeHttp),
        "OpenAI BaseUrl must be a valid HTTP or HTTPS URL.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.EmbeddingModel),
        "OpenAI embedding model is required.")
    .Validate(
        options => options.Dimensions > 0,
        "Embedding dimensions must be greater than zero.")
    .ValidateOnStart();

// Register OpenAI embedding service
builder.Services.AddHttpClient<
    IEmbeddingService,
    OpenAIEmbeddingService>((serviceProvider, httpClient) =>
    {
        var options = serviceProvider
            .GetRequiredService<
               Microsoft.Extensions.Options.IOptions<OpenAIOptions>>()
            .Value;
        httpClient.BaseAddress = new Uri(options.BaseUrl);
        httpClient.Timeout = TimeSpan.FromSeconds(60);
    });

builder.Services
    .AddOptions<QdrantOptions>()
    .Bind(builder.Configuration.GetSection(
        QdrantOptions.SectionName))
    .Validate(
        options => Uri.TryCreate(
            options.BaseUrl,
            UriKind.Absolute,
            out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttps ||
             uri.Scheme == Uri.UriSchemeHttp),
        "Invalid Qdrant BaseUrl.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(
            options.CollectionName),
        "Qdrant collection name is required.")
    .Validate(
        options => options.DenseVectorDimensions > 0,
        "Dense vector dimensions must be greater than zero.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(
            options.DenseVectorName),
        "Dense vector name is required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(
            options.SparseVectorName),
        "Sparse vector name is required.")
    .Validate(
        options => options.DenseVectorName !=
                   options.SparseVectorName,
        "Dense and sparse vector names must be different.")
    .ValidateOnStart();

// Qdrant collection service
builder.Services.AddHttpClient<
    IQdrantCollectionService,
    QdrantCollectionService>((serviceProvider, httpClient) =>
    {
        var options = serviceProvider
            .GetRequiredService<IOptions<QdrantOptions>>()
            .Value;

        httpClient.BaseAddress = new Uri(
            options.BaseUrl.TrimEnd('/') + "/");

        httpClient.Timeout = TimeSpan.FromSeconds(30);

        if (!string.IsNullOrWhiteSpace(options.ApiKey))
        {
            httpClient.DefaultRequestHeaders.Add(
                "api-key",
                options.ApiKey);
        }
    });


// ------------------------------
// Document processing options
// ------------------------------

builder.Services
    .AddOptions<DocumentProcessingOptions>()
    .Bind(builder.Configuration.GetSection(
        DocumentProcessingOptions.SectionName))
    .Validate(
        options => options.ChunkSize > 0,
        "Chunk size must be greater than zero.")
    .Validate(
        options => options.ChunkOverlap >= 0 &&
                   options.ChunkOverlap < options.ChunkSize,
        "Chunk overlap must be non-negative and smaller than chunk size.")
    .Validate(
        options => options.MaxFileSizeBytes > 0,
        "Maximum file size must be greater than zero.")
    .ValidateOnStart();

// ------------------------------
// Document processing services
// ------------------------------

builder.Services.AddScoped<IDocumentTextExtractor,TxtDocumentTextExtractor>();

builder.Services.AddScoped<ITextChunker,
    TextChunker>();

builder.Services.AddScoped<
    IDocumentProcessor,
    DocumentProcessor>();

builder.Services.AddScoped<
    IQdrantSearchStore,
    QdrantSearchStore>();


builder.Services.AddHttpClient<
    IQdrantSearchStore,
    QdrantSearchStore>((sp, client) =>
    {
        var options = sp
            .GetRequiredService<IOptions<QdrantOptions>>()
            .Value;

        client.BaseAddress = new Uri(
            options.BaseUrl.TrimEnd('/') + "/");

        client.Timeout = TimeSpan.FromSeconds(60);

        if (!string.IsNullOrWhiteSpace(options.ApiKey))
        {
            client.DefaultRequestHeaders.Add(
                "api-key",
                options.ApiKey);
        }
    });


builder.Services.AddHttpClient<
    IQdrantPointStore,
    QdrantPointStore>((serviceProvider, httpClient) =>
    {
        var options = serviceProvider
            .GetRequiredService<
                IOptions<QdrantOptions>>()
            .Value;

        httpClient.BaseAddress = new Uri(
            options.BaseUrl.TrimEnd('/') + "/");

        httpClient.Timeout = TimeSpan.FromMinutes(2);

        if (!string.IsNullOrWhiteSpace(options.ApiKey))
        {
            httpClient.DefaultRequestHeaders.Add(
                "api-key",
                options.ApiKey);
        }
    });

// ------------------------------
// Document indexing service
// ------------------------------

builder.Services.AddScoped<
    IDocumentIndexingService,
    DocumentIndexingService>();

builder.Services.AddScoped<
    IDenseSearchService,
    DenseSearchService>();

builder.Services.AddScoped<
    ISparseSearchService,
    SparseSearchService>();

builder.Services.AddScoped<
    IHybridSearchService,
    HybridSearchService>();



var app = builder.Build();


// Map controllers
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();
app.Run();
