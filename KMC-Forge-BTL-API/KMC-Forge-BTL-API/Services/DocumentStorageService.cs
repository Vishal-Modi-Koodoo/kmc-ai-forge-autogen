using Microsoft.AspNetCore.Http;
using KMC_AI_Forge_BTL_Agent.Contracts;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using System;

namespace KMC_AI_Forge_BTL_Agent.Services;

public class DocumentStorageService : IDocumentStorageService
{
    private readonly string _connectionString;
    private readonly string _containerName;

    public DocumentStorageService(IConfiguration configuration)
    {
        _connectionString = configuration["AzureBlobStorage:ConnectionString"] ?? throw new ArgumentNullException("AzureBlobStorage:ConnectionString");
        _containerName = configuration["AzureBlobStorage:ContainerName"] ?? throw new ArgumentNullException("AzureBlobStorage:ContainerName");
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
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
        }

        return blobClient.Uri.ToString();
    }
} 