using Azure;
using KMC_Forge_BTL_Core_Agent.Tools;
using KMC_FOrge_BTL_Models.PDFExtractorResponse;


namespace KMC_Forge_BTL_Core_Agent.Agents
{
    public class DocumentValidatorAgent
    {
        private readonly PdfExtractionTool _pdfExtractionTool;
        private readonly ImageExtractionTool _imageExtractionTool;
        private static string _openAIKey = "";
        private static string _model = "gpt-4.1";
        private static Azure.AI.OpenAI.AzureOpenAIClient _openAIClient = new Azure.AI.OpenAI.AzureOpenAIClient(
     new Uri("https://kmc-ai-forge.openai.azure.com/"),
     new AzureKeyCredential(_openAIKey)
 );

        public DocumentValidatorAgent()
        {
            // Read the analysis prompt from a text file
            string analysisPrompt = System.IO.File.ReadAllText("analysisPrompt.txt");
            _pdfExtractionTool = new PdfExtractionTool(_openAIClient, _model, analysisPrompt);
           // _imageExtractionTool = new ImageExtractionTool();
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

    }
}