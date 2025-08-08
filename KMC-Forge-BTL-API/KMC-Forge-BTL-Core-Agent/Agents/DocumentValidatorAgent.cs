using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Azure;
using KMC_Forge_BTL_Core_Agent.Agents.SubAgents;
using KMC_Forge_BTL_Core_Agent.Tools;
using KMC_Forge_BTL_Core_Agent.Utils;
using KMC_Forge_BTL_Models.PDFExtractorResponse;
using KMC_FOrge_BTL_Models.ImageDataExtractorResponse;
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
        private readonly Azure.AI.OpenAI.AzureOpenAIClient _openAIClient;

        public DocumentValidatorAgent(IConfiguration configuration)
        {
            // Initialize the configuration singleton
            AppConfiguration.Initialize(configuration);
            var config = AppConfiguration.Instance;

            // Initialize OpenAI client using configuration
            _openAIClient = new Azure.AI.OpenAI.AzureOpenAIClient(
                new Uri(config.AzureOpenAIEndpoint),
                new AzureKeyCredential(config.AzureOpenAIApiKey)
            );

            // Read the analysis prompts from configuration paths
            string pdfAnalysisPrompt = File.ReadAllText(config.PdfExtractorPromptPath);
            string imageAnalysisPrompt = File.ReadAllText(config.ImageExtractorPromptPath);

            if (_openAIClient != null)
            {
                _pdfAnalyserAgent = new DocumentAnalyserAgent(_openAIClient, config.AzureOpenAIModel, pdfAnalysisPrompt, "pdf_analyzer").RegisterMessageConnector().RegisterPrintMessage(); 
                _imageAnalyserAgent = new DocumentAnalyserAgent(_openAIClient, config.AzureOpenAIModel, imageAnalysisPrompt, "image_analyzer").RegisterMessageConnector().RegisterPrintMessage(); 
                
                _pdfExtractionTool = new PdfExtractionTool(_pdfAnalyserAgent);
                _imageExtractionTool = new ImageExtractionTool();
            }
        }

        public async Task<CompanyInfo> ExtractDataFromPdfAsync(string path)
        {
            // Delegate PDF extraction to the tool
            return await _pdfExtractionTool.ExtractDataAsync(path);
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