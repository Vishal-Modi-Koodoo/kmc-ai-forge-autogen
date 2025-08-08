using Azure;
using KMC_Forge_BTL_Core_Agent.Tools;
using KMC_Forge_BTL_Models.PDFExtractorResponse;
using KMC_Forge_BTL_Core_Agent.Utils;
using Microsoft.Extensions.Configuration;

namespace KMC_Forge_BTL_Core_Agent.Agents
{
    public class DocumentValidatorAgent
    {
        private readonly PdfExtractionTool _pdfExtractionTool;
        private readonly ImageExtractionTool _imageExtractionTool;
        private readonly DocumentRetrievalTool _documentRetrievalTool;
        private readonly string _openAIKey;
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
            string analysisPrompt = System.IO.File.ReadAllText("/Users/Monish.Koyott/Desktop/KMC-AI-Forge-BTL/kmc-ai-forge-autogen/KMC-Forge-BTL-API/KMC-Forge-BTL-Core-Agent/Prompts/PDFExtractorPrompt.txt");
            
            _pdfExtractionTool = new PdfExtractionTool(_openAIClient, _model, analysisPrompt);
            _imageExtractionTool = new ImageExtractionTool();
            _documentRetrievalTool = new DocumentRetrievalTool();
        }

        public async Task<CompanyInfo> ExtractDataFromPdfAsync(string path)
        {
            // Delegate PDF extraction to the tool
            return await _pdfExtractionTool.ExtractDataAsync(path);
        }

        public async Task<string> ExtractDetailsFromImageAsync(byte[] imageBytes)
        {
            // Delegate image extraction to the tool
            return await _imageExtractionTool.ExtractDetailsAsync(imageBytes);
        }

        public async Task<Stream> ExtractFileFromBlobAsync(string path)
        {
            return await _documentRetrievalTool.RetrieveDocumentAsStreamAsync(path);
        }
    }
}
