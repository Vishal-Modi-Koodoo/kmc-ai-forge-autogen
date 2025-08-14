using Microsoft.AspNetCore.Mvc;
using KMC_AI_Forge_BTL_Agent.Contracts;
using KMC_AI_Forge_BTL_Agent.Models;
using KMC_Forge_BTL_Core_Agent.Agents;
using KMC_AI_Forge_BTL_Agent.AgentInitiatorLayer;
using KMC_Forge_BTL_Models;
using KMC_Forge_BTL_Models.PDFExtractorResponse;
using KMC_Forge_BTL_Models.ImageDataExtractorResponse;
using KMC_Forge_BTL_Models.DBModels;
using KMC_Forge_BTL_Models.DocumentIdentificationResponse;
using KMC_Forge_BTL_Database.Services;
using KMC_Forge_BTL_Database.Repositories;
using KMC_Forge_BTL_Database.Interfaces;
using KMC_Forge_BTL_API.Services;
using KMC_Forge_BTL_API.Hubs;
using KMC_Forge_BTL_API.Enums;
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
    private readonly LeadPortfolioAgent _leadPortfolioAgent;
    private readonly ISignalRNotificationService _signalRNotificationService;

    public DocumentUploadController(IDocumentStorageService documentStorage, 
    IPortfolioValidationService validationService, ILogger<DocumentUploadController> logger, 
    IConfiguration configuration, MongoDbService mongoDbService, IPortfolioUploadRepository portfolioUploadRepository, 
    LeadPortfolioAgent leadPortfolioAgent, ISignalRNotificationService signalRNotificationService){
        _documentStorage = documentStorage;
        _validationService = validationService;
        _logger = logger;
        _configuration = configuration;
        _mongoDbService = mongoDbService;
        _portfolioUploadRepository = portfolioUploadRepository;
        _leadPortfolioAgent = leadPortfolioAgent;
        _signalRNotificationService = signalRNotificationService;
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
    public async Task<IActionResult> UploadPortfolio([FromForm] List<IFormFile> files, [FromForm] string? portfolioId = null)
    {
        portfolioId = portfolioId ?? Guid.NewGuid().ToString();
        var validDocuments = new List<UploadedDocument>();
        var invalidDocuments = new List<InvalidDocumentInfo>();
        var portfolioData = new CompanyInfo();
        var chargesData = new List<ImageExtractionResult>();
        var identifiedDocuments = new List<DocumentProcessingResult>();
        try
        {
            if (files == null || !files.Any())
            {
                return BadRequest("No files uploaded");
            }
            
            var processingStartTime = DateTime.UtcNow;

            // Process each uploaded document with AI identification

            //Step 1: Identify document type
            foreach (var file in files)
            {
                try
                {
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
                    var documentPath = await _documentStorage.StoreDocumentLocally(file, portfolioId);

                    // Process document through LeadPortfolioAgent (includes document type checking and extraction)
                    var identifiedDocument = await _leadPortfolioAgent.IdentifyDocumentType(documentPath, file.FileName, file.Length);
                    identifiedDocuments.Add(identifiedDocument);
                    // Only add to validDocuments if document type is not unknown, else mark as invalid
                    if (identifiedDocument.IdentificationResult.DocumentType == KMC_Forge_BTL_Models.Enums.DocumentType.Unknown)
                    {
                        invalidDocuments.Add(new InvalidDocumentInfo
                        {
                            FileName = file.FileName,
                            ExpectedType = "Unknown",
                            Reason = "Document type could not be identified.",
                            FileSize = file.Length
                        });
                        continue;
                    }
                    
                    validDocuments.Add(new UploadedDocument
                    {
                        DocumentId = Path.GetFileNameWithoutExtension(documentPath),
                        DocumentType = identifiedDocument.IdentificationResult.DocumentType.ToString(),
                        FileName = file.FileName,
                        FilePath = documentPath,
                        ContentType = file.ContentType,
                        FileSize = file.Length,
                        UploadTimestamp = DateTime.UtcNow,
                        PortfolioId = portfolioId
                    });
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

            if(invalidDocuments.Count > 0)
            {
                var failureUpdate = new ProcessingUpdate
                {
                    PortfolioId = portfolioId,
                    Status = "DocumentValidation",
                    Message = $"Document validation failed. {invalidDocuments.Count} invalid documents found.",
                    ProcessingStatus = ProcessingStatus.Failure,
                    ProcessingStep = ProcessingStep.DocumentValidation
                };
                // This is document validation process
                var documentValidationUpdate = new DocumentValidationUpdate
                {
                    PortfolioId = failureUpdate.PortfolioId,
                    Status = failureUpdate.Status,
                    Message = failureUpdate.Message,
                    ProcessingStatus = failureUpdate.ProcessingStatus,
                    ProcessingStep = failureUpdate.ProcessingStep,
                    InvalidDocuments = invalidDocuments.Count,
                    ValidDocuments = validDocuments.Count,
                    TotalDocuments = files.Count,
                    ValidFileNames = validDocuments.Select(v => v.FileName).ToList(),
                    InvalidFileNames = invalidDocuments.Select(i => i.FileName).ToList()
                };
                await _signalRNotificationService.SendDocumentValidationUpdateAsync(portfolioId, documentValidationUpdate);
            }
            else
            {
                var successUpdate = new DocumentValidationUpdate
                {
                    PortfolioId = portfolioId,
                    Status = "DocumentValidation",
                    Message = "All documents updated successfully.",
                    ProcessingStatus = ProcessingStatus.Success,
                    ProcessingStep = ProcessingStep.DocumentValidation,
                    InvalidDocuments = 0,
                    ValidDocuments = validDocuments.Count,
                    TotalDocuments = files.Count,
                    ValidFileNames = validDocuments.Select(v => v.FileName).ToList(),
                    InvalidFileNames = new List<string>()
                };
                await _signalRNotificationService.SendDocumentValidationUpdateAsync(portfolioId, successUpdate);       
            }


            //Step 2: Portfolio Completion
            var portfolioFormData = GetDocumentProcessingResult(identifiedDocuments, KMC_Forge_BTL_Models.Enums.DocumentType.PortfolioForm);

            if (portfolioFormData != null)
            {
                var processingResult = await _leadPortfolioAgent.PortfolioCompletion(portfolioFormData);
                portfolioData = processingResult.PdfData;   
                // send green signal to UI
                var successUpdate = new PortfolioCompletionUpdate
                {
                    PortfolioId = portfolioId,
                    Status = "PortfolioCompletion",
                    Message = "Portfolio data updated successfully.",
                    ProcessingStatus = ProcessingStatus.Success,
                    ProcessingStep = ProcessingStep.PortfolioCompletion,
                    HasPortfolioData = portfolioData != null,
                    CompanyName = portfolioData?.CompanyName,
                    PropertyCount = portfolioData?.Properties?.Count ?? 0
                };
                await _signalRNotificationService.SendPortfolioCompletionUpdateAsync(portfolioId, successUpdate);    
            }
            else
            {
                var failureUpdate = new PortfolioCompletionUpdate
                {
                    PortfolioId = portfolioId,
                    Status = "PortfolioCompletion",
                    Message = "Portfolio data not found.",
                    ProcessingStatus = ProcessingStatus.Failure,
                    ProcessingStep = ProcessingStep.PortfolioCompletion,
                    HasPortfolioData = false,
                    CompanyName = null,
                    PropertyCount = 0
                };
                await _signalRNotificationService.SendPortfolioCompletionUpdateAsync(portfolioId, failureUpdate);
                // send red signal to UI
            }

            //Step 3: Company house validation

            var applicationFormData = GetDocumentProcessingResult(identifiedDocuments, KMC_Forge_BTL_Models.Enums.DocumentType.ApplicationForm);

            if (applicationFormData != null)
            {
                var processingResult = await _leadPortfolioAgent.ValidateCompanyHouseData(applicationFormData);
                chargesData = processingResult.ImageDataList;
                // send green signal to UI
                var successUpdate = new CompanyHouseValidationUpdate
                {
                    PortfolioId = portfolioId,
                    Status = "CompanyHouseValidation",
                    Message = "Company house data updated successfully.",
                    ProcessingStatus = ProcessingStatus.Success,
                    ProcessingStep = ProcessingStep.CompanyHouseValidation,
                    HasCompanyData = chargesData != null && chargesData.Any(),
                    CompanyNumber = null, // You can extract this from chargesData if available
                    ChargeCount = chargesData?.Count ?? 0
                };
                await _signalRNotificationService.SendCompanyHouseValidationUpdateAsync(portfolioId, successUpdate);
            }
            else
            {
                var failureUpdate = new CompanyHouseValidationUpdate
                {
                    PortfolioId = portfolioId,
                    Status = "CompanyHouseValidation",
                    Message = "Company house data not found.",
                    ProcessingStatus = ProcessingStatus.Failure,
                    ProcessingStep = ProcessingStep.CompanyHouseValidation,
                    HasCompanyData = false,
                    CompanyNumber = null,
                    ChargeCount = 0
                };
                await _signalRNotificationService.SendCompanyHouseValidationUpdateAsync(portfolioId, failureUpdate);
                // send red signal to UI
            }

            var processingEndTime = DateTimeOffset.UtcNow;

            // Create extended response with validation results
            string processingTime = (processingEndTime - processingStartTime).TotalMinutes.ToString("F1") + " minutes";
            
            var response = new
            {
                PortfolioId = portfolioId,
                EstimatedProcessingTime = processingTime,
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
            await SavePortfolioData(files, portfolioId, processingTime, portfolioData, chargesData, validDocuments, invalidDocuments);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing portfolio upload");
            return StatusCode(500, new { Error = "Failed to process document upload" });
        }
    }

    private async Task SavePortfolioData(List<IFormFile> files,string portfolioId, string processingTime, CompanyInfo? portfolioData, List<ImageExtractionResult> chargesData, List<UploadedDocument> validDocuments, List<InvalidDocumentInfo> invalidDocuments)
    {
         try
            {
                var database = _mongoDbService?.GetDatabase();
                if (database != null)
                {
                    var portfolioUpload = new KMC_Forge_BTL_Models.DBModels.PortfolioUploadResponse
                    {
                        PortfolioId = portfolioId,
                        EstimatedProcessingTime = processingTime,
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
                            TotalDocuments = files?.Count() ?? 0,
                            ValidDocuments = validDocuments?.Count ?? 0,
                            InvalidDocuments = invalidDocuments?.Count ?? 0,
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

private DocumentProcessingResult? GetDocumentProcessingResult(List<DocumentProcessingResult> identifiedDocuments, KMC_Forge_BTL_Models.Enums.DocumentType documentType)
{
     return identifiedDocuments.Where(x => x.IdentificationResult.DocumentType == documentType
     && x.IsValid && x.IdentificationResult.Confidence >= 0.7 && x.IdentificationResult.IsSuccessful).FirstOrDefault();
    }
}

