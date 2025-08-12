using Microsoft.AspNetCore.Mvc;
using KMC_AI_Forge_BTL_Agent.Contracts;
using KMC_AI_Forge_BTL_Agent.Models;
using KMC_Forge_BTL_Core_Agent.Agents;
<<<<<<< HEAD
=======
using Microsoft.Extensions.Configuration;
using Utilities;
>>>>>>> origin/main

[ApiController]
[Route("api/[controller]")]
public class DocumentUploadController : ControllerBase
{
    private readonly IDocumentStorageService _documentStorage;
    private readonly IPortfolioValidationService _validationService;
    private readonly ILogger<DocumentUploadController> _logger;
    private readonly IConfiguration _configuration;

    public DocumentUploadController(IDocumentStorageService documentStorage, IPortfolioValidationService validationService, ILogger<DocumentUploadController> logger, IConfiguration configuration){
        _documentStorage = documentStorage;
        _validationService = validationService;
        _logger = logger;
        _configuration = configuration;
    }

    /*
    [HttpPost("upload-locally")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadDocumentLocally([FromForm] List<IFormFile> files)
    {
        try
        {
            if (files == null || !files.Any())
            {
                return BadRequest("No files uploaded");
            }

            var uploadedFiles = new List<object>();
            var failedFiles = new List<object>();

            foreach (var file in files)
            {
                try
                {
                    // Validate file size
                    if (file.Length > 50 * 1024 * 1024) // 50MB limit
                    {
                        failedFiles.Add(new
                        {
                            FileName = file.FileName,
                            Error = "File size exceeds 50MB limit",
                            FileSize = file.Length
                        });
                        continue;
                    }

                    // Validate file type
                    if (!IsValidFileType(file))
                    {
                        failedFiles.Add(new
                        {
                            FileName = file.FileName,
                            Error = "Invalid file type. Only PDF, images, and Excel files are supported",
                            FileSize = file.Length
                        });
                        continue;
                    }

                    // Generate a simple portfolio ID for local storage
                    var portfolioId = Guid.NewGuid().ToString();
                    var documentType = "Unknown";

                    // Store document locally
                    var localFilePath = await _documentStorage.StoreDocumentLocally(file, portfolioId, documentType);

                    _logger.LogInformation("Document stored locally: {FileName} at {FilePath}", file.FileName, localFilePath);

                    uploadedFiles.Add(new
                    {
                        FileName = file.FileName,
                        LocalFilePath = localFilePath,
                        FileSize = file.Length,
                        PortfolioId = portfolioId,
                        DocumentType = documentType,
                        UploadTimestamp = DateTimeOffset.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error storing document locally: {FileName}", file.FileName);
                    failedFiles.Add(new
                    {
                        FileName = file.FileName,
                        Error = $"Processing error: {ex.Message}",
                        FileSize = file.Length
                    });
                }
            }

            return Ok(new
            {
                Success = true,
                Message = $"Successfully processed {uploadedFiles.Count} files, {failedFiles.Count} failed",
                UploadedFiles = uploadedFiles,
                FailedFiles = failedFiles,
                Summary = new
                {
                    TotalFiles = files.Count,
                    SuccessfulUploads = uploadedFiles.Count,
                    FailedUploads = failedFiles.Count
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing multiple files upload");
            return StatusCode(500, new { Error = "Failed to process files upload", Details = ex.Message });
        }
    }
    */

    [HttpPost("upload2")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPortfolio([FromForm] List<IFormFile> files)
    {
        var portfolioId = Guid.NewGuid().ToString();
        var validDocuments = new List<UploadedDocument>();
        var invalidDocuments = new List<KMC_Forge_BTL_Models.InvalidDocumentInfo>();

        try
        {
            if (files == null || !files.Any())
            {
                return BadRequest("No files uploaded");
            }

            // Process each uploaded document with AI identification
            foreach (var file in files)
            {
                try
                {
                    // Validate file size
                    if (file.Length > 50 * 1024 * 1024) // 50MB limit
                    {
                        invalidDocuments.Add(new KMC_Forge_BTL_Models.InvalidDocumentInfo
                        {
                            FileName = file.FileName,
                            ExpectedType = "Unknown",
                            Reason = "File size exceeds 50MB limit",
                            FileSize = file.Length
                        });
                        continue;
                    }

                    // Validate file type (basic validation)
                    if (!IsValidFileType(file))
                    {
                        invalidDocuments.Add(new KMC_Forge_BTL_Models.InvalidDocumentInfo
                        {
                            FileName = file.FileName,
                            ExpectedType = "Unknown",
                            Reason = "Invalid file type. Only PDF, images, and Excel files are supported",
                            FileSize = file.Length
                        });
                        continue;
                    }

                    // Store document in Azure Blob Storage with a temporary name
                    var documentPath = await _documentStorage.StoreDocumentLocally(file, portfolioId, "Unknown");

                    // Process document through LeadPortfolioAgent (includes document type checking and extraction)
                    LeadPortfolioAgent leadPortfolioAgent = new LeadPortfolioAgent(_configuration);
                    var processingResult = await leadPortfolioAgent.StartProcessing(documentPath, file.FileName, file.Length);
                    
                    _logger.LogInformation("Document processing completed for {FileName}. Identified as: {IdentifiedType} with confidence: {Confidence}", 
                        file.FileName, processingResult.DocumentType, processingResult.Confidence);

                    if (processingResult.IsValid)
                    {
                        var uploadedDoc = new UploadedDocument
                        {
                            DocumentId = Guid.NewGuid().ToString(),
                            DocumentType = processingResult.DocumentType.ToString(),
                            FileName = file.FileName,
                            FilePath = documentPath,
                            FileSize = file.Length,
                            UploadTimestamp = DateTimeOffset.UtcNow,
                            PortfolioId = portfolioId,
                            ContentType = file.ContentType
                        };
                        validDocuments.Add(uploadedDoc);
                    }
                    else
                    {
                        invalidDocuments.Add(new KMC_Forge_BTL_Models.InvalidDocumentInfo
                        {
                            FileName = file.FileName,
                            ExpectedType = "Unknown",
                            IdentifiedType = processingResult.DocumentType.ToString(),
                            Confidence = processingResult.Confidence,
                            Reason = processingResult.ProcessingMessage,
                            FileSize = file.Length,
                            FilePath = documentPath
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing document {FileName}", file.FileName);
                    invalidDocuments.Add(new KMC_Forge_BTL_Models.InvalidDocumentInfo
                    {
                        FileName = file.FileName,
                        ExpectedType = "Unknown",
                        Reason = $"Processing error: {ex.Message}",
                        FileSize = file.Length
                    });
                }
            }

            _logger.LogInformation("Portfolio processing completed for {PortfolioId}. Valid documents: {ValidCount}, Invalid documents: {InvalidCount}", 
                portfolioId, validDocuments.Count, invalidDocuments.Count);

            // Trigger agent-based validation workflow only for valid documents
            if (validDocuments.Any())
            {
                LeadPortfolioAgent leadPortfolioAgent = new LeadPortfolioAgent(_configuration);
                var firstDocumentUri = validDocuments.FirstOrDefault()?.FilePath;
                if (!string.IsNullOrEmpty(firstDocumentUri))
                {
                    var fileStream = await leadPortfolioAgent.StartDocumentRetrieval(firstDocumentUri);
                }
            }

<<<<<<< HEAD
            // Create extended response with validation results
            var response = new
=======
            if (request.MortgageStatements?.Any() == true)
            {
                foreach (var mortgage in request.MortgageStatements)
                {
                    var mortgageDoc = await ProcessUploadedDocument(mortgage, "MortgageStatement", portfolioId);
                    uploadedDocuments.Add(mortgageDoc);
                }
            }

            _logger.LogInformation("Portfolio validation started for {PortfolioId} with {DocumentCount} documents", 
                portfolioId, uploadedDocuments.Count);

            // LeadPortfolioAgent leadPortfolioAgent = new LeadPortfolioAgent(_configuration);
            // var firstDocumentUri = uploadedDocuments.FirstOrDefault()?.FilePath;
            // if (!string.IsNullOrEmpty(firstDocumentUri))
            // {
            // var fileContent=await leadPortfolioAgent.StartDocumentRetrieval("https://kmcaidocumentrepository.blob.core.windows.net/kmcaidocumentrepository/14135249-d243-453e-bad1-a459ca37c1d5/EquifaxCredit/61debcb0-d9c6-4cec-b35c-cc48fbb84935_testdata.pdf?sp=r&st=2025-08-08T07:53:22Z&se=2025-08-08T16:08:22Z&spr=https&sv=2024-11-04&sr=b&sig=3vlxbNJu7%2FGpD%2FSiiwZUVplLWsrvgmD6QxPHb0leTCs%3D");
            // var fileStream = await leadPortfolioAgent.StartProcessing(fileContent,"https://i.ibb.co/n8r20Zq9/screencapture-find-and-updatepany-information-service-gov-uk-company-12569527-charges-TPa-d-WITwye-o.png");
            // }

            // GetCompanyHouseDetails getCompanyHouseDetails = new GetCompanyHouseDetails();
            // await getCompanyHouseDetails.CaptureAllIncludingChargesAsync();

            // await leadPortfolioAgent.StartProcessing("C:/Users/VishalModi/Desktop/testdata.pdf");
            return Ok(new PortfolioUploadResponse
>>>>>>> origin/main
            {
                PortfolioId = portfolioId,
                EstimatedProcessingTime = "3-5 minutes",
                Status = "Processing",
                UploadedDocuments = validDocuments,
                InvalidDocuments = invalidDocuments,
                Summary = new
                {
                    TotalDocuments = files.Count(),
                    ValidDocuments = validDocuments.Count,
                    InvalidDocuments = invalidDocuments.Count,
                    ProcessingCompleted = true
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing portfolio upload");
            return StatusCode(500, new { Error = "Failed to process document upload" });
        }
    }

    private bool IsValidFileType(IFormFile file)
    {
        var allowedTypes = new[] 
        { 
            "application/pdf", 
            "image/jpeg", 
            "image/png", 
            "image/gif",
            "image/bmp",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "application/vnd.ms-excel",
            "text/csv",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/msword"
        };

        return allowedTypes.Contains(file.ContentType);
    }

}

