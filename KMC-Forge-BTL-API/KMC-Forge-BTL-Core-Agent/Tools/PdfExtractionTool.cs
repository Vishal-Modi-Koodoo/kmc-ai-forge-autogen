using AutoGen;
using AutoGen.Core;
using AutoGen.OpenAI;
using KMC_Forge_BTL_Configurations;
using KMC_Forge_BTL_Core_Agent.Utils;
using KMC_Forge_BTL_Models.PDFExtractorResponse;

namespace KMC_Forge_BTL_Core_Agent.Tools
{
    public class PdfExtractionTool 
    {
        private readonly MiddlewareStreamingAgent<OpenAIChatAgent> _pdfAnalyserAgent;
        private readonly AppConfiguration _config;

        public PdfExtractionTool(MiddlewareStreamingAgent<OpenAIChatAgent> pdfAnalyserAgent)
        {
            _pdfAnalyserAgent = pdfAnalyserAgent;
            _config = AppConfiguration.Instance;
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

            // Retry logic with exponential backoff using configuration
            int maxRetries = _config.MaxRetries;
            int currentRetry = 0;
            
            while (currentRetry < maxRetries)
            {
                try
                {
                    var messages = await userProxy.InitiateChatAsync(
                        receiver: _pdfAnalyserAgent,
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
                catch (Exception ex) when (ex.Message.Contains("rate limit") || ex.Message.Contains("429"))
                {
                    currentRetry++;
                    if (currentRetry >= maxRetries)
                    {
                        Console.WriteLine($"Rate limit exceeded after {maxRetries} retries. Returning empty result.");
                        return new CompanyInfo();
                    }
                    
                    int delayMs = (int)Math.Pow(2, currentRetry) * _config.RetryDelayMs; // Use configured delay
                    Console.WriteLine($"Rate limit hit. Retrying in {delayMs/1000} seconds... (Attempt {currentRetry}/{maxRetries})");
                    await Task.Delay(delayMs);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                    return new CompanyInfo();
                }
            }

            return new CompanyInfo();
        }
    }
}