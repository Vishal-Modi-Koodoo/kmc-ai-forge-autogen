using Microsoft.AspNetCore.Mvc;
using KMC_AI_Forge_BTL_Agent.Contracts;
using KMC_AI_Forge_BTL_Agent.Models;
using KMC_Forge_BTL_Core_Agent.Agents;
using KMC_AI_Forge_BTL_Agent.AgentInitiatorLayer;
using KMC_Forge_BTL_Models;
using KMC_Forge_BTL_Models.PDFExtractorResponse;
using KMC_Forge_BTL_Models.ImageDataExtractorResponse;
using KMC_Forge_BTL_Models.DBModels;
using KMC_Forge_BTL_Database.Services;
using KMC_Forge_BTL_Database.Repositories;
using KMC_Forge_BTL_Database.Interfaces;
using UploadedDocument = KMC_AI_Forge_BTL_Agent.Models.UploadedDocument;

[ApiController]
[Route("api/[controller]")]
public class DocumentUploadController : ControllerBase
{
    private readonly IDocumentStorageService _documentStorage;
    private readonly IPortfolioValidationService _validationService;
    private readonly ILogger<DocumentUploadController> _logger;
    private readonly IConfiguration _configuration;
    private readonly MongoDbService _mongoDbService;
    private readonly IPortfolioUploadRepository _portfolioUploadRepository;


    public DocumentUploadController(IDocumentStorageService documentStorage, 
    IPortfolioValidationService validationService, ILogger<DocumentUploadController> logger, 
    IConfiguration configuration, MongoDbService mongoDbService, IPortfolioUploadRepository portfolioUploadRepository){
        _documentStorage = documentStorage;
        _validationService = validationService;
        _logger = logger;
        _configuration = configuration;
        _mongoDbService = mongoDbService;
        _portfolioUploadRepository = portfolioUploadRepository;
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
        var invalidDocuments = new List<InvalidDocumentInfo>();
        var portfolioData = new CompanyInfo();
        var chargesData = new List<ImageExtractionResult>();
        try
        {
            if (files == null || !files.Any())
            {
                return BadRequest("No files uploaded");
            }
            var processingStartTime = DateTime.UtcNow;

            // Process each uploaded document with AI identification
            foreach (var file in files)
            {
                try
                {
                    // Validate file size
                    if (file.Length > 50 * 1024 * 1024) // 50MB limit
                    {
                        invalidDocuments.Add(new InvalidDocumentInfo
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
                        invalidDocuments.Add(new InvalidDocumentInfo
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
                    // INSERT_YOUR_CODE
                    
                    if (!chargesData.Any() && processingResult?.ImageDataList.Any() == true)
                    {
                        chargesData = processingResult?.ImageDataList ?? new List<ImageExtractionResult>();
                    }

                    // INSERT_YOUR_CODE
                    if (portfolioData.CompanyName == null && processingResult?.PdfData?.CompanyName != null)
                    {
                        portfolioData = processingResult?.PdfData ?? new CompanyInfo();
                    }
                

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
                        invalidDocuments.Add(new InvalidDocumentInfo
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
                    invalidDocuments.Add(new InvalidDocumentInfo
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
            // if (validDocuments.Any())
            // {
            //     LeadPortfolioAgent leadPortfolioAgent = new LeadPortfolioAgent(_configuration);
            //     var firstDocumentUri = validDocuments.FirstOrDefault()?.FilePath;
            //     if (!string.IsNullOrEmpty(firstDocumentUri))
            //     {
            //         var fileStream = await leadPortfolioAgent.StartDocumentRetrieval(firstDocumentUri);
            //     }
            // }
            var processingEndTime = DateTimeOffset.UtcNow;

            // Create extended response with validation results

            
            var response = new
            {
                PortfolioId = portfolioId,
                EstimatedProcessingTime = (processingEndTime - processingStartTime).TotalMinutes.ToString("F1") + " minutes",
                Status = "Done",
                PortfolioData = portfolioData,
                ChargesData = chargesData,
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

            // INSERT_YOUR_CODE

            // Save the upload summary to MongoDB
            try
            {
                var database = _mongoDbService?.GetDatabase();
                if (database != null)
                {
                    var portfolioUpload = new PortfolioUploadResponse
                    {
                        PortfolioId = portfolioId,
                        EstimatedProcessingTime = (processingEndTime - processingStartTime).TotalMinutes.ToString("F1") + " minutes",
                        Status = "Done",
                        PortfolioData = new CompanyInfoCollection
                        {
                            CompanyName = portfolioData?.CompanyName ?? "Unknown",
                            Properties = portfolioData?.Properties?.Select(p => new PropertyInfoCollection
                            {
                                PropertyAddress = p.PropertyAddress,
                                PropertyType = p.PropertyType,
                                YearPurchased = p.YearPurchased,
                                CurrentEstimatedValue = p.CurrentEstimatedValue,
                                RentalIncomePerMonth = p.RentalIncomePerMonth,
                                MortgagePaymentPerMonth = p.MortgagePaymentPerMonth,
                                Owner = p.Owner,
                                Lender = p.Lender,
                                DateOfMortgage = p.DateOfMortgage,
                                MortgageBalanceOutstanding = p.MortgageBalanceOutstanding,
                                AnnualServiceCharge = p.AnnualServiceCharge,
                                AnnualGroundRent = p.AnnualGroundRent,
                                CompanyInfoId = portfolioId,
                                PortfolioId = portfolioId,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            }).ToList() ?? new List<PropertyInfoCollection>(),
                            DocumentId = portfolioId
                        },
                        ChargesData = chargesData?.Select(c => new ImageExtractionResultCollection
                        {
                            PersonsEntitled = c.PersonsEntitled,
                            BriefDescription = c.BriefDescription,
                            PortfolioId = portfolioId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }).ToList() ?? new List<ImageExtractionResultCollection>(),
                        UploadedDocuments = validDocuments?.Select(doc => new UploadedDocumentCollection
                        {
                            DocumentId = doc.DocumentId,
                            DocumentType = doc.DocumentType,
                            FileName = doc.FileName,
                            FilePath = doc.FilePath,
                            ContentType = doc.ContentType,
                            FileSize = doc.FileSize,
                            UploadTimestamp = doc.UploadTimestamp,
                            PortfolioId = doc.PortfolioId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }).ToList() ?? new List<UploadedDocumentCollection>(),
                        InvalidDocuments = invalidDocuments?.Select(doc => new InvalidDocumentInfoCollection
                        {
                            FileName = doc.FileName,
                            ExpectedType = doc.ExpectedType,
                            IdentifiedType = doc.IdentifiedType,
                            Confidence = doc.Confidence,
                            Reason = doc.Reason,
                            FileSize = doc.FileSize,
                            FilePath = doc.FilePath,
                            PortfolioId = portfolioId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }).ToList() ?? new List<InvalidDocumentInfoCollection>(),
                        Summary = new UploadSummary
                        {
                            TotalDocuments = files.Count(),
                            ValidDocuments = validDocuments.Count,
                            InvalidDocuments = invalidDocuments.Count,
                            ProcessingCompleted = true
                        },
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _portfolioUploadRepository.CreateAsync(portfolioUpload);
                }
                else
                {
                    _logger.LogWarning("MongoDB database instance is null. Skipping MongoDB insert.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert portfolio upload summary into MongoDB for PortfolioId {PortfolioId}", portfolioId);
            }

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

