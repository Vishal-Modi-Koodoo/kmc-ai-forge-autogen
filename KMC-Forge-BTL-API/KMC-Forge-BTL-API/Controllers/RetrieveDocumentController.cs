using Microsoft.AspNetCore.Mvc;
using KMC_AI_Forge_BTL_Agent.Contracts;

[ApiController]
[Route("api/[controller]")]
public class RetrieveDocumentController : ControllerBase
{
    private readonly IDocumentRetrievalService _documentRetrievalService;
    private readonly ILogger<RetrieveDocumentController> _logger;

    public RetrieveDocumentController(IDocumentRetrievalService documentRetrievalService, ILogger<RetrieveDocumentController> logger)
    {
        _documentRetrievalService = documentRetrievalService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a document by its URI
    /// </summary>
    /// <param name="documentUri">The URI of the document to retrieve</param>
    /// <returns>The document as a file download</returns>
    [HttpGet("retrieve")]
    public async Task<IActionResult> RetrieveDocument([FromQuery] string documentUri)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(documentUri))
            {
                return BadRequest("Document URI is required");
            }

            _logger.LogInformation("Retrieving document: {DocumentUri}", documentUri);

            // Check if document exists
            if (!await _documentRetrievalService.DocumentExistsAsync(documentUri))
            {
                _logger.LogWarning("Document not found: {DocumentUri}", documentUri);
                return NotFound($"Document not found: {documentUri}");
            }

            // Download the document
            var (content, contentType, fileName) = await _documentRetrievalService.DownloadDocumentAsync(documentUri);

            _logger.LogInformation("Successfully retrieved document: {DocumentUri}", documentUri);

            return File(content, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document: {DocumentUri}", documentUri);
            return StatusCode(500, new { Error = "Failed to retrieve document", Details = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves all documents for a specific portfolio
    /// </summary>
    /// <param name="portfolioId">The portfolio identifier</param>
    /// <returns>List of document URIs for the portfolio</returns>
    [HttpGet("portfolio/{portfolioId}")]
    public async Task<IActionResult> GetDocumentsByPortfolio(string portfolioId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(portfolioId))
            {
                return BadRequest("Portfolio ID is required");
            }

            _logger.LogInformation("Retrieving documents for portfolio: {PortfolioId}", portfolioId);

            var documents = await _documentRetrievalService.GetDocumentsByPortfolioAsync(portfolioId);

            _logger.LogInformation("Found {DocumentCount} documents for portfolio: {PortfolioId}", documents.Count, portfolioId);

            return Ok(new
            {
                PortfolioId = portfolioId,
                DocumentCount = documents.Count,
                Documents = documents
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documents for portfolio: {PortfolioId}", portfolioId);
            return StatusCode(500, new { Error = "Failed to retrieve documents", Details = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves documents for a specific portfolio and document type
    /// </summary>
    /// <param name="portfolioId">The portfolio identifier</param>
    /// <param name="documentType">The type of document to retrieve</param>
    /// <returns>List of document URIs for the portfolio and document type</returns>
    [HttpGet("portfolio/{portfolioId}/type/{documentType}")]
    public async Task<IActionResult> GetDocumentsByPortfolioAndType(string portfolioId, string documentType)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(portfolioId))
            {
                return BadRequest("Portfolio ID is required");
            }

            if (string.IsNullOrWhiteSpace(documentType))
            {
                return BadRequest("Document type is required");
            }

            _logger.LogInformation("Retrieving {DocumentType} documents for portfolio: {PortfolioId}", documentType, portfolioId);

            var documents = await _documentRetrievalService.GetDocumentsByPortfolioAndTypeAsync(portfolioId, documentType);

            _logger.LogInformation("Found {DocumentCount} {DocumentType} documents for portfolio: {PortfolioId}", 
                documents.Count, documentType, portfolioId);

            return Ok(new
            {
                PortfolioId = portfolioId,
                DocumentType = documentType,
                DocumentCount = documents.Count,
                Documents = documents
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving {DocumentType} documents for portfolio: {PortfolioId}", documentType, portfolioId);
            return StatusCode(500, new { Error = "Failed to retrieve documents", Details = ex.Message });
        }
    }

    /// <summary>
    /// Downloads a specific document with custom filename
    /// </summary>
    /// <param name="documentUri">The URI of the document to download</param>
    /// <param name="fileName">Optional custom filename for the download</param>
    /// <returns>The document as a file download</returns>
    [HttpGet("download")]
    public async Task<IActionResult> DownloadDocument([FromQuery] string documentUri, [FromQuery] string? fileName = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(documentUri))
            {
                return BadRequest("Document URI is required");
            }

            _logger.LogInformation("Downloading document: {DocumentUri} with filename: {FileName}", documentUri, fileName ?? "default");

            // Check if document exists
            if (!await _documentRetrievalService.DocumentExistsAsync(documentUri))
            {
                _logger.LogWarning("Document not found: {DocumentUri}", documentUri);
                return NotFound($"Document not found: {documentUri}");
            }

            // Download the document with custom filename
            var (content, contentType, actualFileName) = await _documentRetrievalService.DownloadDocumentAsync(documentUri, fileName);

            _logger.LogInformation("Successfully downloaded document: {DocumentUri}", documentUri);

            return File(content, contentType, actualFileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document: {DocumentUri}", documentUri);
            return StatusCode(500, new { Error = "Failed to download document", Details = ex.Message });
        }
    }

    /// <summary>
    /// Checks if a document exists
    /// </summary>
    /// <param name="documentUri">The URI of the document to check</param>
    /// <returns>Boolean indicating if the document exists</returns>
    [HttpGet("exists")]
    public async Task<IActionResult> DocumentExists([FromQuery] string documentUri)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(documentUri))
            {
                return BadRequest("Document URI is required");
            }

            var exists = await _documentRetrievalService.DocumentExistsAsync(documentUri);

            _logger.LogInformation("Document existence check for {DocumentUri}: {Exists}", documentUri, exists);

            return Ok(new
            {
                DocumentUri = documentUri,
                Exists = exists
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking document existence: {DocumentUri}", documentUri);
            return StatusCode(500, new { Error = "Failed to check document existence", Details = ex.Message });
        }
    }

    /// <summary>
    /// Gets document metadata without downloading the content
    /// </summary>
    /// <param name="documentUri">The URI of the document</param>
    /// <returns>Document metadata including content type and filename</returns>
    [HttpGet("metadata")]
    public async Task<IActionResult> GetDocumentMetadata([FromQuery] string documentUri)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(documentUri))
            {
                return BadRequest("Document URI is required");
            }

            _logger.LogInformation("Retrieving metadata for document: {DocumentUri}", documentUri);

            // Check if document exists
            if (!await _documentRetrievalService.DocumentExistsAsync(documentUri))
            {
                _logger.LogWarning("Document not found: {DocumentUri}", documentUri);
                return NotFound($"Document not found: {documentUri}");
            }

            // Get document metadata by downloading and immediately disposing
            var (content, contentType, fileName) = await _documentRetrievalService.DownloadDocumentAsync(documentUri);
            
            // Dispose the content stream since we only need metadata
            await content.DisposeAsync();

            _logger.LogInformation("Successfully retrieved metadata for document: {DocumentUri}", documentUri);

            return Ok(new
            {
                DocumentUri = documentUri,
                ContentType = contentType,
                FileName = fileName,
                Exists = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document metadata: {DocumentUri}", documentUri);
            return StatusCode(500, new { Error = "Failed to retrieve document metadata", Details = ex.Message });
        }
    }
} 