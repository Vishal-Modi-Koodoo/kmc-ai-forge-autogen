using AutoGen;
using AutoGen.Core;
using AutoGen.OpenAI;
using KMC_Forge_BTL_Core_Agent.Utils;
using KMC_Forge_BTL_Models.PDFExtractorResponse;

namespace KMC_Forge_BTL_Core_Agent.Tools
{
    public class PdfExtractionTool
    {
        private readonly MiddlewareStreamingAgent<OpenAIChatAgent> _pDFAnalyserAgent;

        // Constructor to initialize the PDFAnalyserAgent
        public PdfExtractionTool(MiddlewareStreamingAgent<OpenAIChatAgent> pDFAnalyserAgent)
        {
            _pDFAnalyserAgent = pDFAnalyserAgent;
        }

        public async Task<CompanyInfo> ExtractDataAsync(string fileContent)
        {
            // string extractedText = PdfExtractor.ExtractTextFromPdf(fileContent);

            if (string.IsNullOrWhiteSpace(fileContent))
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

            try
            {
                var messages = await userProxy.InitiateChatAsync(
                    receiver: _pDFAnalyserAgent,
                    message: fileContent,
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
            catch (Exception ex)
            {
                Console.WriteLine("Error extracting data from PDF: " + ex.Message);
                return new CompanyInfo();
            }
        }
    }
}