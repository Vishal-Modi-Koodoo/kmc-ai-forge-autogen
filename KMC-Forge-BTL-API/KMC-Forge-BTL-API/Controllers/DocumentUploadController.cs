using Microsoft.AspNetCore.Mvc;
using KMC_AI_Forge_BTL_Agent.Contracts;
using KMC_AI_Forge_BTL_Agent.Models;
using KMC_Forge_BTL_Core_Agent.Agents;

[ApiController]
[Route("api/[controller]")]
public class DocumentUploadController : ControllerBase
{
    private readonly IDocumentStorageService _documentStorage;
    private readonly IPortfolioValidationService _validationService;
    private readonly ILogger<DocumentUploadController> _logger;

    public DocumentUploadController(IDocumentStorageService documentStorage, IPortfolioValidationService validationService, ILogger<DocumentUploadController> logger){
        _documentStorage = documentStorage;
        _validationService = validationService;
        _logger = logger;
    }

    [HttpPost("upload-portfolio")]
    public async Task<IActionResult> UploadPortfolio([FromForm] PortfolioUploadRequest request)
    {
        var portfolioId = Guid.NewGuid().ToString();
        var uploadedDocuments = new List<UploadedDocument>();

        try
        {
            // Validate required documents
            if (request.ApplicationForm == null)
                return BadRequest("Application Form (FMA) is required");
                
            if (request.EquifaxCreditSearch == null)
                return BadRequest("Equifax Commercial Credit Search is required");

            // Process each uploaded document type
            var fmaDoc = await ProcessUploadedDocument(request.ApplicationForm, "FMA", portfolioId);
            uploadedDocuments.Add(fmaDoc);

            var equifaxDoc = await ProcessUploadedDocument(request.EquifaxCreditSearch, "EquifaxCredit", portfolioId);
            uploadedDocuments.Add(equifaxDoc);

            // Optional documents
            if (request.PortfolioForm != null)
            {
                var portfolioDoc = await ProcessUploadedDocument(request.PortfolioForm, "PortfolioForm", portfolioId);
                uploadedDocuments.Add(portfolioDoc);
            }

            if (request.ASTAgreements?.Any() == true)
            {
                foreach (var ast in request.ASTAgreements)
                {
                    var astDoc = await ProcessUploadedDocument(ast, "AST", portfolioId);
                    uploadedDocuments.Add(astDoc);
                }
            }

            if (request.BankStatements?.Any() == true)
            {
                foreach (var statement in request.BankStatements)
                {
                    var bankDoc = await ProcessUploadedDocument(statement, "BankStatement", portfolioId);
                    uploadedDocuments.Add(bankDoc);
                }
            }

            if (request.MortgageStatements?.Any() == true)
            {
                foreach (var mortgage in request.MortgageStatements)
                {
                    var mortgageDoc = await ProcessUploadedDocument(mortgage, "MortgageStatement", portfolioId);
                    uploadedDocuments.Add(mortgageDoc);
                }
            }

            // Trigger agent-based validation workflow
            var validationResult = await _validationService.StartValidation(portfolioId, uploadedDocuments);

            _logger.LogInformation("Portfolio validation started for {PortfolioId} with {DocumentCount} documents", 
                portfolioId, uploadedDocuments.Count);

            LeadPortfolioAgent leadPortfolioAgent = new LeadPortfolioAgent();
            // Use the first uploaded document URI for retrieval
            var firstDocumentUri = uploadedDocuments.FirstOrDefault()?.FilePath;
            if (!string.IsNullOrEmpty(firstDocumentUri))
            {
                var fileStream = await leadPortfolioAgent.OrchestrateDocumentValidationAsync("/Users/Monish.Koyott/Desktop/KMC-AI-Forge-BTL/kmc-ai-forge-autogen/KMC-Forge-BTL-API/KMC-Forge-BTL-Core-Agent/UploadedFiles/testdata.pdf");
            }

            return Ok(new PortfolioUploadResponse
            {
                PortfolioId = portfolioId,
                ValidationId = validationResult.ValidationId,
                EstimatedProcessingTime = "3-5 minutes",
                Status = "Processing",
                UploadedDocuments = uploadedDocuments
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing portfolio upload");
            return StatusCode(500, new { Error = "Failed to process document upload" });
        }
    }

    private async Task<UploadedDocument> ProcessUploadedDocument(IFormFile file, string documentType, string portfolioId)
    {
        // Validate file type and size
        if (!IsValidDocumentType(file, documentType))
            throw new ArgumentException($"Invalid file type for {documentType}");

        if (file.Length > 50 * 1024 * 1024) // 50MB limit
            throw new ArgumentException("File size exceeds 50MB limit");

        // Store document in Azure Blob Storage
        var documentPath = await _documentStorage.StoreDocument(file, portfolioId, documentType);

        return new UploadedDocument
        {
            DocumentId = Guid.NewGuid().ToString(),
            DocumentType = documentType,
            FileName = file.FileName,
            FilePath = documentPath,
            FileSize = file.Length,
            UploadTimestamp = DateTimeOffset.UtcNow,
            PortfolioId = portfolioId,
            ContentType = file.ContentType
        };
    }

    private bool IsValidDocumentType(IFormFile file, string documentType)
    {
        var allowedTypes = documentType switch
        {
            "FMA" or "PortfolioForm" or "EquifaxCredit" or "AST" or "MortgageStatement" => 
                new[] { "application/pdf", "image/jpeg", "image/png", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            "BankStatement" => 
                new[] { "application/pdf", "text/csv", "application/vnd.ms-excel" },
            _ => new[] { "application/pdf" }
        };

        return allowedTypes.Contains(file.ContentType);
    }
}