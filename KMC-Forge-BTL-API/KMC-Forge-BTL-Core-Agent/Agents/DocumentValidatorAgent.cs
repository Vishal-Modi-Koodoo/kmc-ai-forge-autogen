using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Azure;
using KMC_Forge_BTL_Core_Agent.Agents.SubAgents;
using KMC_Forge_BTL_Core_Agent.Tools;
using KMC_Forge_BTL_Core_Agent.Utils;
using KMC_Forge_BTL_Models.PDFExtractorResponse;
using Microsoft.Extensions.Configuration;


namespace KMC_Forge_BTL_Core_Agent.Agents
{
    public class DocumentValidatorAgent
    {
        private readonly PdfExtractionTool _pdfExtractionTool;
        private readonly MiddlewareStreamingAgent<OpenAIChatAgent> _pdfAnalyserAgent;
        private readonly ImageExtractionTool _imageExtractionTool;
        private readonly DocumentRetrievalTool _documentRetrievalTool;
        private readonly string _openAIKey = "";
        private readonly string _model = "gpt-4.1";
        private readonly Azure.AI.OpenAI.AzureOpenAIClient _openAIClient;

        public DocumentValidatorAgent(IConfiguration configuration)
        {
            // Get OpenAI API key from configuration
            _openAIKey = configuration["AzureAI:ApiKey"] ?? throw new ArgumentNullException("AzureAI:ApiKey");
            
            // Initialize OpenAI client
            _openAIClient = new Azure.AI.OpenAI.AzureOpenAIClient(
                new Uri("https://kmc-ai-forge.openai.azure.com/"),
                new AzureKeyCredential(_openAIKey)
            );

            // Read the analysis prompt from a text file
            string analysisPrompt = File.ReadAllText("Prompts/PDFExtractorPrompt.txt");

            if (_openAIClient != null)
            {
                // Read the analysis prompt from a text file
                _pdfAnalyserAgent = new PDFAnalyserAgent(_openAIClient, _model, analysisPrompt).RegisterMessageConnector().RegisterPrintMessage(); 
                _pdfExtractionTool = new PdfExtractionTool(_pdfAnalyserAgent);
                // _imageExtractionTool = new ImageExtractionTool();
               _documentRetrievalTool = new DocumentRetrievalTool();
            }
        }

        public async Task<CompanyInfo> ExtractDataFromPdfAsync(string path)
        {
            // Delegate PDF extraction to the tool
            return await _pdfExtractionTool.ExtractDataAsync(path);
        }

        public async Task<string> ExtractDetailsFromImageAsync(byte[] imageBytes)
        {
            // Delegate image extraction to the tool
            //return await _imageExtractionTool.ExtractDetailsAsync(imageBytes);
            return "";
        }

        public async Task<string> ExtractFileFromBlobAsync(string path)
        {
            return await _documentRetrievalTool.RetrieveDocumentAsync(path);
        }
    }
}