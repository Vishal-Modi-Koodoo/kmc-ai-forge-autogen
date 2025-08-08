using Azure.Storage.Blobs;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using KMC_AI_Forge_BTL_Agent.Contracts;

namespace KMC_Forge_BTL_API.Services;

public class DocumentRetrievalService : IDocumentRetrievalService
{
    private readonly string _connectionString;
    private readonly string _containerName;

    public DocumentRetrievalService(IConfiguration configuration)
    {
        _connectionString = configuration["AzureBlobStorage:ConnectionString"] ?? throw new ArgumentNullException("AzureBlobStorage:ConnectionString");
        _containerName = configuration["AzureBlobStorage:ContainerName"] ?? throw new ArgumentNullException("AzureBlobStorage:ContainerName");
    }

    public async Task<string> RetrieveDocumentAsync(string documentUri)
    {
        string text = "";
        try
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            
            // Extract blob name from URI
            var uri = new Uri(documentUri);
            var blobName = uri.AbsolutePath.TrimStart('/').Replace($"{_containerName}/", "");
            var blobClient = containerClient.GetBlobClient(blobName);

            // Check if blob exists
            if (!await blobClient.ExistsAsync())
            {
                throw new FileNotFoundException($"Document not found: {documentUri}");
            }

            // Download blob to memory stream
        using (HttpClient httpClient = new HttpClient())
        using (Stream pdfStream = await httpClient.GetStreamAsync(uri))
        using (MemoryStream memoryStream = new MemoryStream())
        {
            // Copy to memory stream to allow seeking
            await pdfStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using (PdfReader reader = new PdfReader(memoryStream))
            using (PdfDocument pdf = new PdfDocument(reader))
            {
                for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
                {
                    var strategy = new SimpleTextExtractionStrategy();
                    text = PdfTextExtractor.GetTextFromPage(pdf.GetPage(i), strategy);
                    Console.WriteLine($"Page {i}:\n{text}\n");
                }
            }
        }
        return text;
    }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve document: {ex.Message}", ex);
        }
    }

    public async Task<List<string>> GetDocumentsByPortfolioAndTypeAsync(string portfolioId, string documentType)
    {
        try
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            
            var documentUris = new List<string>();
            var prefix = $"{portfolioId}/{documentType}/";
            
            await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: prefix))
            {
                var blobClient = containerClient.GetBlobClient(blobItem.Name);
                documentUris.Add(blobClient.Uri.ToString());
            }
            
            return documentUris;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve documents for portfolio {portfolioId} and type {documentType}: {ex.Message}", ex);
        }
    }

    public async Task<List<string>> GetDocumentsByPortfolioAsync(string portfolioId)
    {
        try
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            
            var documentUris = new List<string>();
            var prefix = $"{portfolioId}/";
            
            await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: prefix))
            {
                var blobClient = containerClient.GetBlobClient(blobItem.Name);
                documentUris.Add(blobClient.Uri.ToString());
            }
            
            return documentUris;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve documents for portfolio {portfolioId}: {ex.Message}", ex);
        }
    }

    public async Task<(Stream Content, string ContentType, string FileName)> DownloadDocumentAsync(string documentUri, string? fileName = null)
    {
        try
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            
            // Extract blob name from URI
            var uri = new Uri(documentUri);
            var blobName = uri.AbsolutePath.TrimStart('/').Replace($"{_containerName}/", "");
            var blobClient = containerClient.GetBlobClient(blobName);

            // Check if blob exists
            if (!await blobClient.ExistsAsync())
            {
                throw new FileNotFoundException($"Document not found: {documentUri}");
            }

            // Get blob properties
            var properties = await blobClient.GetPropertiesAsync();
            var contentType = properties.Value.ContentType;
            
            // Use provided filename or extract from blob name
            var actualFileName = fileName ?? Path.GetFileName(blobName);
            
            // Download blob to memory stream
            var memoryStream = new MemoryStream();
            await blobClient.DownloadToAsync(memoryStream);
            memoryStream.Position = 0;
            
            return (memoryStream, contentType, actualFileName);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to download document: {ex.Message}", ex);
        }
    }

    public async Task<bool> DocumentExistsAsync(string documentUri)
    {
        try
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            
            // Extract blob name from URI
            var uri = new Uri(documentUri);
            var blobName = uri.AbsolutePath.TrimStart('/').Replace($"{_containerName}/", "");
            var blobClient = containerClient.GetBlobClient(blobName);

            return await blobClient.ExistsAsync();
        }
        catch (Exception)
        {
            return false;
        }
    }
} 