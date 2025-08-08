using Microsoft.AspNetCore.Http;

namespace KMC_AI_Forge_BTL_Agent.Contracts;

public interface IDocumentStorageService
{
    /// <summary>
    /// Stores a document in the configured storage system
    /// </summary>
    /// <param name="file">The file to store</param>
    /// <param name="portfolioId">The portfolio identifier</param>
    /// <param name="documentType">The type of document being stored</param>
    /// <returns>The path/identifier of the stored document</returns>
    Task<string> StoreDocument(IFormFile file, string portfolioId, string documentType);
}
