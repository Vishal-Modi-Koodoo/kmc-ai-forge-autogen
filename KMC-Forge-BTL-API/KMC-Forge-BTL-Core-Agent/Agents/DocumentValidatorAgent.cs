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
        private readonly CompanyNumberExtractorTool _companyNumberExtractorTool;
        private readonly MiddlewareStreamingAgent<OpenAIChatAgent> _pdfAnalyserAgent;
        private readonly MiddlewareStreamingAgent<OpenAIChatAgent> _imageAnalyserAgent;
        private readonly MiddlewareStreamingAgent<OpenAIChatAgent> _companyNumberExtractorAgent;
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
            string companyNumberExtractorPrompt = File.ReadAllText(config.CompanyNumberExtractorPromptPath);

            if (_openAIClient != null)
            {
                _pdfAnalyserAgent = new DocumentAnalyserAgent(_openAIClient, config.AzureOpenAIModel, pdfAnalysisPrompt, "pdf_analyzer").RegisterMessageConnector().RegisterPrintMessage(); 
                _imageAnalyserAgent = new DocumentAnalyserAgent(_openAIClient, config.AzureOpenAIModel, imageAnalysisPrompt, "image_analyzer").RegisterMessageConnector().RegisterPrintMessage(); 
                
                _pdfExtractionTool = new PdfExtractionTool(_pdfAnalyserAgent);
                _imageExtractionTool = new ImageExtractionTool();
                
                // Create a document identification agent for the tool
                var documentIdentificationAgent = new DocumentAnalyserAgent(_openAIClient, config.AzureOpenAIModel, documentIdentifierPrompt, "document_identifier").RegisterMessageConnector().RegisterPrintMessage();
                _documentIdentificationTool = new DocumentIdentificationTool(documentIdentificationAgent);
                
                // Create a company number extractor agent and tool
                _companyNumberExtractorAgent = new DocumentAnalyserAgent(_openAIClient, config.AzureOpenAIModel, companyNumberExtractorPrompt, "company_number_extractor").RegisterMessageConnector().RegisterPrintMessage();
                _companyNumberExtractorTool = new CompanyNumberExtractorTool(_companyNumberExtractorAgent);
            }
        }

        public async Task<DocumentProcessingResult> IdentifyDocumentType(string filePath, string fileName, long fileSize)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                {
                    return new DocumentProcessingResult
                    {
                        IsValid = false,
                        DocumentType = KMC_Forge_BTL_Models.Enums.DocumentType.Unknown,
                        Confidence = 0.0,
                        DocumentContent = "",
                        PdfData = null,
                        ImageDataList = new List<ImageExtractionResult>(),
                        CompanyNumber = "",
                        FilePath = filePath,
                        FileName = fileName,
                        FileSize = fileSize,
                        ProcessingMessage = "File does not exist or path is invalid"
                    };
                }

                // Step 2: Process document identification
                var identificationResult = await DocumentIdentificationProcess(filePath, fileName, fileSize);
                return new DocumentProcessingResult
                {
                    IsValid = true,
                    DocumentType = identificationResult.DocumentType,
                    Confidence = identificationResult.Confidence,
                    DocumentContent = identificationResult.DocumentContent,
                    IdentificationResult = identificationResult,
                    FilePath = filePath,
                    FileName = fileName,
                    FileSize = fileSize,
                    ProcessingMessage = $"Document successfully identified as {identificationResult.DocumentType}"
                };
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
                    ImageDataList = new List<ImageExtractionResult>(),
                    CompanyNumber = "",
                    FilePath = filePath,
                    FileName = fileName,
                    FileSize = fileSize,
                    ProcessingMessage = $"Error identifying document type: {ex.Message}"
                };
            }
        }

        public async Task<DocumentProcessingResult> PortfolioCompletion(DocumentProcessingResult identificationResult)
        {
            try
            {
                // Step 3: Check if document type is valid (not Unknown)
                // Step 4: Document is valid, proceed with extraction based on document type and file type
                CompanyInfo? pdfData = null;
                
                // Extract PDF data for PortfolioForm documents
                pdfData = await _pdfExtractionTool.ExtractDataAsync(identificationResult.DocumentContent);
                    
                return new DocumentProcessingResult
                {   
                    IsValid = true,
                    DocumentType = identificationResult.DocumentType,
                    Confidence = identificationResult.Confidence,
                    DocumentContent = identificationResult.DocumentContent,
                    PdfData = pdfData,
                    ImageDataList = new List<ImageExtractionResult>(),
                    CompanyNumber = "",
                    FilePath = identificationResult.FilePath,
                    FileName = identificationResult.FileName,
                    FileSize = identificationResult.FileSize,
                    IdentificationResult = identificationResult.IdentificationResult,
                    ProcessingMessage = $"Document successfully identified as {identificationResult.DocumentType}"
                };
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
                    ImageDataList = new List<ImageExtractionResult>(),
                    CompanyNumber = "",
                    IdentificationResult = identificationResult.IdentificationResult,
                    ProcessingMessage = $"Processing error: {ex.Message}"
                };
            }
        }

        private async Task<DocumentIdentificationProcessResult> DocumentIdentificationProcess(string filePath, string fileName, long fileSize)
        {
            string documentContent = "";
            string fileExtension = Path.GetExtension(filePath).ToLower();

            // Extract text content based on file type
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
                    return new DocumentIdentificationProcessResult
                    {
                        IsSuccessful = false,
                        DocumentType = KMC_Forge_BTL_Models.Enums.DocumentType.Unknown,
                        Confidence = 0.0,
                        DocumentContent = "",
                        ErrorMessage = $"Unsupported file type: {fileExtension}"
                    };
            }

            // Identify document type using AI (for text-based files)
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
                return new DocumentIdentificationProcessResult
                {
                    IsSuccessful = false,
                    DocumentType = KMC_Forge_BTL_Models.Enums.DocumentType.Unknown,
                    Confidence = 0.0,
                    DocumentContent = "",
                    ErrorMessage = "No text could be extracted from the document"
                };
            }

            return new DocumentIdentificationProcessResult
            {
                IsSuccessful = identificationResult.IsSuccessful,
                DocumentType = identificationResult.DocumentType,
                Confidence = identificationResult.Confidence,
                DocumentContent = documentContent,
                ErrorMessage = identificationResult.ErrorMessage
            };
        }

        public async Task<DocumentProcessingResult> ValidateCompanyHouseData(DocumentProcessingResult identificationResult)
        {
            try
            {
                List<ImageExtractionResult> imageDataList = new List<ImageExtractionResult>();
                string companyNumber = "";

            var companyNumberResult = await _companyNumberExtractorTool.ExtractCompanyNumberAsync(identificationResult.DocumentContent);
                        if (companyNumberResult.IsSuccessful)
                        {
                           companyNumber = companyNumberResult.CompanyNumber;
                            CompanyHouseDetailsCapturer companyHouseDetailsCapturer = new CompanyHouseDetailsCapturer(companyNumber);
                            var path = await companyHouseDetailsCapturer.CaptureAllIncludingChargesAsync();

                            var screenshotsBasePath = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
                            var companyPath = Path.Combine(screenshotsBasePath, companyNumber);
                            var chargesPath = Path.Combine(companyPath, "Charges");
                            var chargeLinksPath = Path.Combine(chargesPath, "Charge_Links");

                            var chargeLinks = Directory.GetFiles(chargeLinksPath, "*.png", SearchOption.AllDirectories);
                            foreach (var chargeLink in chargeLinks)
                            {
                                try
                                {
                                    var extractedImageData = await _imageExtractionTool.ExtractDataAsync(chargeLink);
                                    imageDataList.Add(extractedImageData);
                                    Console.WriteLine($"Extracted image data from {chargeLink}: {extractedImageData.PersonsEntitled}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error extracting image data from {chargeLink}: {ex.Message}");
                                }
                            }
                        }

                return new DocumentProcessingResult
                {
                    IsValid = true,
                    DocumentType = KMC_Forge_BTL_Models.Enums.DocumentType.PortfolioForm,
                    Confidence = 1.0,
                    DocumentContent = identificationResult.DocumentContent,
                    PdfData = identificationResult.PdfData,
                    ImageDataList = imageDataList,
                    CompanyNumber = companyNumber,
                    FilePath = identificationResult.FilePath,
                    FileName = identificationResult.FileName,
                    FileSize = identificationResult.FileSize,
                    IdentificationResult = identificationResult.IdentificationResult,
                    ProcessingMessage = "Company house data validated successfully"
                };
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
                    ImageDataList = new List<ImageExtractionResult>(),
                    CompanyNumber = "",
                    FilePath = identificationResult.FilePath,
                    FileName = identificationResult.FileName,
                    FileSize = identificationResult.FileSize,
                    IdentificationResult = identificationResult.IdentificationResult,
                    ProcessingMessage = $"Error validating company house data: {ex.Message}"
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

}