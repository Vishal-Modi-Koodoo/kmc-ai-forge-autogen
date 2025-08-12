using Microsoft.AspNetCore.Http;
using KMC_AI_Forge_BTL_Agent.Contracts;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace KMC_AI_Forge_BTL_Agent.Services;

public class DocumentStorageService : IDocumentStorageService
{
    private readonly string _connectionString;
    private readonly string _containerName;
    private readonly string _localStoragePath;

    public DocumentStorageService(IConfiguration configuration)
    {
        _connectionString = configuration["AzureBlobStorage:ConnectionString"] ?? throw new ArgumentNullException("AzureBlobStorage:ConnectionString");
        _containerName = configuration["AzureBlobStorage:ContainerName"] ?? throw new ArgumentNullException("AzureBlobStorage:ContainerName");
        _localStoragePath = configuration["LocalStorage:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
    }

    public async Task<string> StoreDocument(IFormFile file, string portfolioId, string documentType)
    {
        var blobServiceClient = new BlobServiceClient(_connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

        var blobName = $"{portfolioId}/{documentType}/{Guid.NewGuid()}_{file.FileName}";
        var blobClient = containerClient.GetBlobClient(blobName);

        using (var stream = file.OpenReadStream())
        {
            // await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
        }

        return blobClient.Uri.ToString();
    }

    public async Task<string> StoreDocumentLocally(IFormFile file, string portfolioId, string documentType)
    {
        try
        {
            // Create the directory structure: Uploads/PortfolioId/DocumentType/
            var portfolioDirectory = Path.Combine(_localStoragePath, portfolioId);
            var documentTypeDirectory = Path.Combine(portfolioDirectory, documentType);
            
            // Ensure directories exist
            Directory.CreateDirectory(documentTypeDirectory);

            // Generate unique filename with timestamp and GUID
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var guid = Guid.NewGuid().ToString("N");
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"{timestamp}_{guid}{fileExtension}";
            
            // Full path for the file
            var filePath = Path.Combine(documentTypeDirectory, fileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return filePath;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to store document locally: {ex.Message}", ex);
        }
    }
} 