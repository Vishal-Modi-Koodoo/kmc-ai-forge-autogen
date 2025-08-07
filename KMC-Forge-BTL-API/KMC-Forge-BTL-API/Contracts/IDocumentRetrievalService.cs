namespace KMC_AI_Forge_BTL_Agent.Contracts;

public interface IDocumentRetrievalService
{
    /// <summary>
    /// Retrieves a document from Azure Blob Storage by its URI
    /// </summary>
    /// <param name="documentUri">The URI of the document to retrieve</param>
    /// <returns>The document as a stream</returns>
    Task<Stream> RetrieveDocumentAsync(string documentUri);

    /// <summary>
    /// Retrieves a document from Azure Blob Storage by portfolio ID and document type
    /// </summary>
    /// <param name="portfolioId">The portfolio identifier</param>
    /// <param name="documentType">The type of document to retrieve</param>
    /// <returns>A list of document URIs for the specified portfolio and document type</returns>
    Task<List<string>> GetDocumentsByPortfolioAndTypeAsync(string portfolioId, string documentType);

    /// <summary>
    /// Retrieves all documents for a specific portfolio
    /// </summary>
    /// <param name="portfolioId">The portfolio identifier</param>
    /// <returns>A list of all document URIs for the specified portfolio</returns>
    Task<List<string>> GetDocumentsByPortfolioAsync(string portfolioId);

    /// <summary>
    /// Downloads a document and returns it as a file result
    /// </summary>
    /// <param name="documentUri">The URI of the document to download</param>
    /// <param name="fileName">Optional custom filename for the download</param>
    /// <returns>The document as a file stream with metadata</returns>
    Task<(Stream Content, string ContentType, string FileName)> DownloadDocumentAsync(string documentUri, string? fileName = null);

    /// <summary>
    /// Checks if a document exists in Azure Blob Storage
    /// </summary>
    /// <param name="documentUri">The URI of the document to check</param>
    /// <returns>True if the document exists, false otherwise</returns>
    Task<bool> DocumentExistsAsync(string documentUri);
} 