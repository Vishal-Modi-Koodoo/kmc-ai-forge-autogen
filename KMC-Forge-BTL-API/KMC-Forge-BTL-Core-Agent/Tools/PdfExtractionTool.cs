using AutoGen;
using AutoGen.Core;
using AutoGen.OpenAI;
using Azure.AI.OpenAI;
using KMC_Forge_BTL_Core_Agent.Utils;
using KMC_Forge_BTL_Models.PDFExtractorResponse;

namespace KMC_Forge_BTL_Core_Agent.Tools
{
    public class PdfExtractionTool : OpenAIChatAgent
    {
        public PdfExtractionTool(AzureOpenAIClient openAIClient, string model, string analysisPrompt)
            : base(
                name: "pdf_analyzer",
                systemMessage: analysisPrompt,
                chatClient: openAIClient.GetChatClient(model))
        {
        }

        public async Task<CompanyInfo> ExtractDataAsync(string path)
        {
            string extractedText = PdfExtractor.ExtractTextFromPdf(path);

            if (string.IsNullOrWhiteSpace(extractedText))
            {
                Console.WriteLine("No text could be extracted from the PDF.");
                return new CompanyInfo();
            }

            var userProxy = new UserProxyAgent(
                name: "user",
                systemMessage: "check the values",
                defaultReply: "Thank you for the output",
                humanInputMode: HumanInputMode.NEVER)
                .RegisterPrintMessage();

            Console.WriteLine("\nAnalyzing PDF content with AI...\n");

            try{
              var messages = await userProxy.InitiateChatAsync(
                 receiver: this,
                 message: extractedText,
                 maxRound: 1);

            string aiJson = null;
            foreach (var message in messages)
            {
                if (message is TextMessage textMessage && textMessage.Role == Role.Assistant)
                {
                    aiJson = textMessage.Content;
                }
            }

            var companyInfo = System.Text.Json.JsonSerializer.Deserialize<CompanyInfo>(aiJson);
            return companyInfo;
           }
           catch(Exception ex){
            Console.WriteLine("Error extracting data from PDF: " + ex.Message);
            return new CompanyInfo();
           }
        }
    }
}