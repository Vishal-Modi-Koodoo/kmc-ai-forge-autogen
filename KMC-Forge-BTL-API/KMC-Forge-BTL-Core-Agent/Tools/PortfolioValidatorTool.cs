using System.Text.Json;
using AutoGen;
using AutoGen.Core;
using AutoGen.OpenAI;
using KMC_Forge_BTL_Configurations;
using KMC_Forge_BTL_Models.PDFExtractorResponse;

namespace KMC_Forge_BTL_Core_Agent.Tools
{
    public class PortfolioValidatorTool
    {
        private readonly MiddlewareStreamingAgent<OpenAIChatAgent> _pdfAnalyserAgent;
        private readonly AppConfiguration _config;
        public PortfolioValidatorTool(MiddlewareStreamingAgent<OpenAIChatAgent> pdfAnalyserAgent)
        {
           _pdfAnalyserAgent = pdfAnalyserAgent;
            _config = AppConfiguration.Instance; 
        }

         public async Task<string> ValidatePortfolioAsync(string companyInfo)
        {
            if (string.IsNullOrWhiteSpace(companyInfo))
            {
                Console.WriteLine("No text could be extracted from the PDF.");
                return "Error: CompanyInfo is null or empty";
            }

            var userProxy = new UserProxyAgent(
                name: "user",
                systemMessage: "check the values",
                defaultReply: "Thank you for the output",
                humanInputMode: HumanInputMode.NEVER)
                .RegisterPrintMessage();

            Console.WriteLine("\nValidating Portfolio with AI...\n");

            // Retry logic with exponential backoff using configuration
            int maxRetries = _config.MaxRetries;
            int currentRetry = 0;
            
            while (currentRetry <= maxRetries)
            {
                try
                {
                    var messages = await userProxy.InitiateChatAsync(
                        receiver: _pdfAnalyserAgent,
                        message: companyInfo,
                        maxRound: 1);

                    string aiJson = null;
                    foreach (var message in messages)
                    {
                        if (message is TextMessage textMessage && textMessage.Role == Role.Assistant)
                        {
                            aiJson = textMessage.Content;
                            break; // Exit loop immediately when we get data
                        }
                    }

                    if (!string.IsNullOrEmpty(aiJson))
                    {
                        return aiJson; // Return immediately, don't continue the loop
                    }
                    
                    // If we get here, no valid response was received
                    return "Error: No response received from AI agent";
                }
                catch (Exception ex) when (ex.Message.Contains("rate limit") || ex.Message.Contains("429"))
                {
                    currentRetry++;
                    if (currentRetry > maxRetries)
                    {
                        Console.WriteLine($"Rate limit exceeded after {maxRetries} retries. Returning error.");
                        return "Error: Rate limit exceeded after maximum retries";
                    }
                    
                    int delayMs = (int)Math.Pow(2, currentRetry) * _config.RetryDelayMs;
                    Console.WriteLine($"Rate limit hit. Retrying in {delayMs/1000} seconds... (Attempt {currentRetry}/{maxRetries})");
                    await Task.Delay(delayMs);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                    return $"Unexpected error: {ex.Message}";
                }
            }

            return "Error: Maximum retries exceeded";
        }
    
    }
}