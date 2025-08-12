using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Azure;
using KMC_Forge_BTL_Core_Agent.Agents.SubAgents;
using KMC_Forge_BTL_Core_Agent.Tools;
using KMC_Forge_BTL_Core_Agent.Utils;
using KMC_Forge_BTL_Models.PDFExtractorResponse;
using KMC_Forge_BTL_Models.DocumentIdentificationResponse;
using Microsoft.Extensions.Configuration;
using KMC_Forge_BTL_Configurations;
using KMC_Forge_BTL_Models.ImageDataExtractorResponse;

namespace KMC_Forge_BTL_Core_Agent.Agents
{
    public class DocumentValidatorAgent
    {
        private readonly PdfExtractionTool _pdfExtractionTool;
        private readonly MiddlewareStreamingAgent<OpenAIChatAgent> _pdfAnalyserAgent;
        private readonly MiddlewareStreamingAgent<OpenAIChatAgent> _imageAnalyserAgent;
        private readonly ImageExtractionTool _imageExtractionTool;
        private readonly DocumentRetrievalTool _documentRetrievalTool;
        private readonly DocumentIdentificationTool _documentIdentificationTool;
        private readonly Azure.AI.OpenAI.AzureOpenAIClient _openAIClient;

        public DocumentValidatorAgent(IConfiguration configuration)
        {
            _documentRetrievalTool = new DocumentRetrievalTool();
            // Initialize the configuration singleton
            AppConfiguration.Initialize(configuration);
            var config = AppConfiguration.Instance;

            // Initialize OpenAI client using configuration
            _openAIClient = new Azure.AI.OpenAI.AzureOpenAIClient(
                new Uri(config.AzureOpenAIEndpoint),
                new AzureKeyCredential(config.AzureOpenAIApiKey)
            );

            // Read the analysis prompts from configuration paths
            string pdfAnalysisPrompt = File.ReadAllText(config.PdfDataExtractorPromptPath);
            string imageAnalysisPrompt = File.ReadAllText(config.ImageDataExtractorPromptPath);
            string documentIdentifierPrompt = File.ReadAllText(config.DocumentIdentifierPromptPath);

            if (_openAIClient != null)
            {
                _pdfAnalyserAgent = new DocumentAnalyserAgent(_openAIClient, config.AzureOpenAIModel, pdfAnalysisPrompt, "pdf_analyzer").RegisterMessageConnector().RegisterPrintMessage(); 
                _imageAnalyserAgent = new DocumentAnalyserAgent(_openAIClient, config.AzureOpenAIModel, imageAnalysisPrompt, "image_analyzer").RegisterMessageConnector().RegisterPrintMessage(); 
                
                _pdfExtractionTool = new PdfExtractionTool(_pdfAnalyserAgent);
                _imageExtractionTool = new ImageExtractionTool();
                
                // Create a document identification agent for the tool
                var documentIdentificationAgent = new DocumentAnalyserAgent(_openAIClient, config.AzureOpenAIModel, documentIdentifierPrompt, "document_identifier").RegisterMessageConnector().RegisterPrintMessage();
                _documentIdentificationTool = new DocumentIdentificationTool(documentIdentificationAgent);
            }
        }

        public async Task<DocumentProcessingResult> StartProcessing(string filePath, string fileName, long fileSize)
        {
            try
            {
                // Step 1: Validate file exists and extract content
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                {
                    return new DocumentProcessingResult
                    {
                        IsValid = false,
                        DocumentType = KMC_Forge_BTL_Models.Enums.DocumentType.Unknown,
                        Confidence = 0.0,
                        DocumentContent = "",
                        PdfData = null,
                        ImageData = null,
                        FilePath = filePath,
                        FileName = fileName,
                        FileSize = fileSize,
                        ProcessingMessage = "File does not exist or path is invalid"
                    };
                }

                string documentContent = "";
                string fileExtension = Path.GetExtension(filePath).ToLower();

                // Step 2: Extract text content based on file type
                switch (fileExtension)
                {
                    case ".pdf":
                        documentContent = PdfExtractor.ExtractTextFromPdf(filePath);
                        break;
                    case ".txt":
                        documentContent = await File.ReadAllTextAsync(filePath);
                        break;
                    case ".jpg":
                    case ".jpeg":
                    case ".png":
                    case ".gif":
                    case ".bmp":
                        // For images, we'll extract content later in the process
                        documentContent = "";
                        break;
                    default:
                        return new DocumentProcessingResult
                        {
                            IsValid = false,
                            DocumentType = KMC_Forge_BTL_Models.Enums.DocumentType.Unknown,
                            Confidence = 0.0,
                            DocumentContent = "",
                            PdfData = null,
                            ImageData = null,
                            FilePath = filePath,
                            FileName = fileName,
                            FileSize = fileSize,
                            ProcessingMessage = $"Unsupported file type: {fileExtension}"
                        };
                }

                // Step 3: Identify document type using AI (for text-based files)
                DocumentIdentificationResult identificationResult;
                if (!string.IsNullOrWhiteSpace(documentContent))
                {
                    identificationResult = await _documentIdentificationTool.IdentifyDocumentTypeAsync(documentContent);
                }
                else if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png" || fileExtension == ".gif" || fileExtension == ".bmp")
                {
                    // For images, we'll process them directly without text extraction
                    identificationResult = new DocumentIdentificationResult
                    {
                        DocumentType = KMC_Forge_BTL_Models.Enums.DocumentType.Unknown,
                        Confidence = 0.8, // Assume valid for processing
                        IsSuccessful = true
                    };
                }
                else
                {
                    return new DocumentProcessingResult
                    {
                        IsValid = false,
                        DocumentType = KMC_Forge_BTL_Models.Enums.DocumentType.Unknown,
                        Confidence = 0.0,
                        DocumentContent = "",
                        PdfData = null,
                        ImageData = null,
                        FilePath = filePath,
                        FileName = fileName,
                        FileSize = fileSize,
                        ProcessingMessage = "No text could be extracted from the document"
                    };
                }

                // Step 4: Check if document type is valid (not Unknown)
                if (identificationResult.IsSuccessful && identificationResult.Confidence >= 0.7 && identificationResult.DocumentType != KMC_Forge_BTL_Models.Enums.DocumentType.Unknown)
                {
                    // Step 5: Document is valid, proceed with extraction based on document type and file type
                    CompanyInfo? pdfData = null;
                    ImageExtractionResult? imageData = null;
                    
                    // Only extract PDF data for PortfolioForm documents
                    if (identificationResult.DocumentType == KMC_Forge_BTL_Models.Enums.DocumentType.PortfolioForm && 
                        filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        // Extract PDF data using the already extracted content
                        pdfData = await _pdfExtractionTool.ExtractDataAsync(documentContent);
                        CompanyHouseDetailsCapturer companyHouseDetailsCapturer = new CompanyHouseDetailsCapturer("03489004");
                        var path = await companyHouseDetailsCapturer.CaptureAllIncludingChargesAsync();
                    }
                    else if (filePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || 
                             filePath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) || 
                             filePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                             filePath.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                             filePath.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                    {
                        // Extract image data
                        imageData = await _imageExtractionTool.ExtractDataAsync(filePath);
                    }
                    
                    return new DocumentProcessingResult
                    {
                        IsValid = true,
                        DocumentType = identificationResult.DocumentType,
                        Confidence = identificationResult.Confidence,
                        DocumentContent = documentContent,
                        PdfData = pdfData,
                        ImageData = imageData,
                        FilePath = filePath,
                        FileName = fileName,
                        FileSize = fileSize,
                        ProcessingMessage = $"Document successfully identified as {identificationResult.DocumentType}"
                    };
                }
                else
                {
                    // Step 6: Document is invalid or could not be identified
                    return new DocumentProcessingResult
                    {
                        IsValid = false,
                        DocumentType = identificationResult.DocumentType,
                        Confidence = identificationResult.Confidence,
                        DocumentContent = documentContent,
                        PdfData = null,
                        ImageData = null,
                        FilePath = filePath,
                        FileName = fileName,
                        FileSize = fileSize,
                        ProcessingMessage = identificationResult.ErrorMessage ?? "Document type could not be determined or confidence too low"
                    };
                }
            }
            catch (Exception ex)
            {
                return new DocumentProcessingResult
                {
                    IsValid = false,
                    DocumentType = KMC_Forge_BTL_Models.Enums.DocumentType.Unknown,
                    Confidence = 0.0,
                    DocumentContent = "",
                    PdfData = null,
                    ImageData = null,
                    FilePath = filePath,
                    FileName = fileName,
                    FileSize = fileSize,
                    ProcessingMessage = $"Processing error: {ex.Message}"
                };
            }
        }

        public async Task<CompanyInfo> ExtractDataFromFileAsync(string filePath)
        {
            // Delegate data extraction to the tool
            string fileContent = PdfExtractor.ExtractTextFromPdf(filePath);
            return await _pdfExtractionTool.ExtractDataAsync(fileContent);
        }

        public async Task<ImageExtractionResult> ExtractDetailsFromImageAsync(string path)
        {
            // Delegate image extraction to the tool
            return await _imageExtractionTool.ExtractDataAsync(path);
        }

        public async Task<string> ExtractFileFromBlobAsync(string path)
        {
            return await _documentRetrievalTool.RetrieveDocumentAsync(path);
        }
    }

    public class DocumentProcessingResult
    {
        public bool IsValid { get; set; }
        public KMC_Forge_BTL_Models.Enums.DocumentType DocumentType { get; set; }
        public double Confidence { get; set; }
        public string DocumentContent { get; set; } = "";
        public KMC_Forge_BTL_Models.PDFExtractorResponse.CompanyInfo? PdfData { get; set; }
        public ImageExtractionResult? ImageData { get; set; }
        public string FilePath { get; set; } = "";
        public string FileName { get; set; } = "";
        public long FileSize { get; set; }
        public string ProcessingMessage { get; set; } = "";
    }
}