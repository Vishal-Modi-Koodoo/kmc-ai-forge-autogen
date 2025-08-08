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


namespace KMC_Forge_BTL_Core_Agent.Agents
{
    public class DocumentValidatorAgent
    {
        private readonly PdfExtractionTool _pdfExtractionTool;
        private readonly MiddlewareStreamingAgent<OpenAIChatAgent> _pdfAnalyserAgent;
        private readonly MiddlewareStreamingAgent<OpenAIChatAgent> _imageAnalyserAgent;
        private readonly ImageExtractionTool _imageExtractionTool;
        private readonly DocumentRetrievalTool _documentRetrievalTool;
        private readonly string _openAIKey = "";
        private readonly string _model = "gpt-4.1";
        private readonly Azure.AI.OpenAI.AzureOpenAIClient _openAIClient;

        public DocumentValidatorAgent(IConfiguration configuration)
        {
            // Initialize OpenAI client if API key is available
            if (!string.IsNullOrWhiteSpace(_openAIKey))
            {
                _openAIClient = new Azure.AI.OpenAI.AzureOpenAIClient(
     new Uri("https://kmc-ai-forge.openai.azure.com/"),
     new AzureKeyCredential(_openAIKey)
 );
            }

            // Read the analysis prompt from a text file
            string pdfAnalysisPrompt = File.ReadAllText("Prompts/PDFExtractorPrompt.txt");
            string imageAnalysisPrompt = File.ReadAllText("Prompts/ImageExtractorPrompt.txt");

            if (_openAIClient != null)
            {
                _pdfAnalyserAgent = new DocumentAnalyserAgent(_openAIClient, _model, pdfAnalysisPrompt, "pdf_analyzer").RegisterMessageConnector().RegisterPrintMessage(); 
                _imageAnalyserAgent = new DocumentAnalyserAgent(_openAIClient, _model, pdfAnalysisPrompt, "image_analyzer").RegisterMessageConnector().RegisterPrintMessage(); 
                
                _pdfExtractionTool = new PdfExtractionTool(_pdfAnalyserAgent);
                _imageExtractionTool = new ImageExtractionTool(_imageAnalyserAgent);
            }
        }

        public async Task<CompanyInfo> ExtractDataFromPdfAsync(string path)
        {
            // Delegate PDF extraction to the tool
            return await _pdfExtractionTool.ExtractDataAsync(path);
        }

        public async Task<ChargeInfo> ExtractDetailsFromImageAsync(string path)
        {
            // Delegate image extraction to the tool
            return await _imageExtractionTool.ExtractDataAsync(path);
        }

        public async Task<Stream> ExtractFileFromBlobAsync(string path)
        {
            return await _documentRetrievalTool.RetrieveDocumentAsStreamAsync(path);
        }
    }
}