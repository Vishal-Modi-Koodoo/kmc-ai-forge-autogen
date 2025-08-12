using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Azure;
using KMC_Forge_BTL_Core_Agent.Agents.SubAgents;
using KMC_Forge_BTL_Core_Agent.Tools;
using KMC_Forge_BTL_Core_Agent.Utils;
using KMC_Forge_BTL_Models.PDFExtractorResponse;
using KMC_FOrge_BTL_Models.ImageDataExtractorResponse;
using KMC_Forge_BTL_Models.DocumentIdentificationResponse;
using Microsoft.Extensions.Configuration;
using KMC_Forge_BTL_Configurations;

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
                // Step 1: Identify document type using AI
                var identificationResult = await _documentIdentificationTool.IdentifyDocumentTypeFromFileAsync(filePath);
                
                // Step 2: Check if document type is valid (not Unknown)
                if (identificationResult.IsSuccessful && identificationResult.Confidence >= 0.7 && identificationResult.DocumentType != KMC_Forge_BTL_Models.Enums.DocumentType.Unknown)
                {
                    // Step 3: Document is valid, proceed with extraction based on file type
                    string documentContent = "";
                    CompanyInfo? pdfData = null;
                    ImageExtractionResult? imageData = null;
                    
                    if (filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        // Extract PDF data
                        pdfData = await _pdfExtractionTool.ExtractDataAsync(filePath);
                       // documentContent = await _documentRetrievalTool.RetrieveDocumentAsync(filePath);
                    }
                    else if (filePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || 
                             filePath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) || 
                             filePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    {
                        // Extract image data
                        imageData = await _imageExtractionTool.ExtractDataAsync(filePath);
                        //documentContent = await _documentRetrievalTool.RetrieveDocumentAsync(filePath);
                    }
                    else
                    {
                        // For other file types, just retrieve content
                       // documentContent = await _documentRetrievalTool.RetrieveDocumentAsync(filePath);
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
                    // Step 4: Document is invalid or could not be identified
                    return new DocumentProcessingResult
                    {
                        IsValid = false,
                        DocumentType = identificationResult.DocumentType,
                        Confidence = identificationResult.Confidence,
                        DocumentContent = "",
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

        public async Task<CompanyInfo> ExtractDataFromContentAsync(string content)
        {
            // Delegate data extraction to the tool
            return await _pdfExtractionTool.ExtractDataAsync(content);
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
        public KMC_FOrge_BTL_Models.ImageDataExtractorResponse.ImageExtractionResult? ImageData { get; set; }
        public string FilePath { get; set; } = "";
        public string FileName { get; set; } = "";
        public long FileSize { get; set; }
        public string ProcessingMessage { get; set; } = "";
    }
}