using AutoGen.Core;
using Azure;
using KMC_Forge_BTL_Core_Agent.Tools;
using KMC_Forge_BTL_Core_Agent.Utils;
using AutoGen;


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
            _imageExtractionTool = new ImageExtractionTool();
        }

        public async Task<string> ExtractDataFromPdfAsync(string path)
        {
            // Delegate PDF extraction to the tool
            return await ExtractDataAsync(path);
        }

        public async Task<string> ExtractDetailsFromImageAsync(byte[] imageBytes)
        {
            // Delegate image extraction to the tool
            return await _imageExtractionTool.ExtractDetailsAsync(imageBytes);
        }

        public async Task<string> ExtractDataAsync(string path)
        {
            // Delegate PDF extraction to the tool
            string extractedText = PdfExtractor.ExtractTextFromPdf(path);

            if (string.IsNullOrWhiteSpace(extractedText))
            {
                Console.WriteLine("No text could be extracted from the PDF.");
                return "";
            }

            //        var pdfAnalyzer = new PdfAnalysisAgent(openAIClient, model, analysisPrompt)
            //.RegisterMessageConnector()
            //.RegisterPrintMessage();

            var userProxy = new UserProxyAgent(
       name: "user",
       systemMessage: "check the values",
       defaultReply: "Thank you for the output",
       humanInputMode: HumanInputMode.NEVER)// Set to NEVER for automated processing
       .RegisterPrintMessage();


            Console.WriteLine("\nAnalyzing PDF content with AI...\n");

            // Prepare the message with extracted PDF content


            // Start the conversation
            var messages = await userProxy.InitiateChatAsync(
                receiver: _pdfExtractionTool,
                message: extractedText,
                maxRound: 1);

            // Find the assistant agent response that contains JSON
            string aiJson = null;
            foreach (var message in messages)
            {
                if (message is TextMessage textMessage && textMessage.Role == Role.Assistant)
                {
                    aiJson = textMessage.Content;
                }
            }

            return aiJson ?? "No valid JSON response from AI.";
        }
    }
}