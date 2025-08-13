using AutoGen;
using AutoGen.Core;
using AutoGen.OpenAI;
using KMC_Forge_BTL_Configurations;

namespace KMC_Forge_BTL_Core_Agent.Tools
{
    public class CompanyNumberExtractorTool 
    {
        private readonly MiddlewareStreamingAgent<OpenAIChatAgent> _companyNumberExtractorAgent;
        private readonly AppConfiguration _config;

        public CompanyNumberExtractorTool(MiddlewareStreamingAgent<OpenAIChatAgent> companyNumberExtractorAgent)
        {
            _companyNumberExtractorAgent = companyNumberExtractorAgent;
            _config = AppConfiguration.Instance;
        }

        public async Task<CompanyNumberResult> ExtractCompanyNumberAsync(string fileContent)
        {
            if (string.IsNullOrWhiteSpace(fileContent))
            {
                Console.WriteLine("No text could be extracted from the PDF.");
                return new CompanyNumberResult { IsSuccessful = false, ErrorMessage = "No content found in PDF" };
            }

            var userProxy = new UserProxyAgent(
                name: "user",
                systemMessage: "Extract the company registration number from the application form",
                defaultReply: "Thank you for the output",
                humanInputMode: HumanInputMode.NEVER)
                .RegisterPrintMessage();

            Console.WriteLine("\nExtracting company number from Application Form...\n");

            // Retry logic with exponential backoff using configuration
            int maxRetries = _config.MaxRetries;
            int currentRetry = 0;
            
            while (currentRetry < maxRetries)
            {
                try
                {
                    var messages = await userProxy.InitiateChatAsync(
                        receiver: _companyNumberExtractorAgent,
                        message: fileContent,
                        maxRound: 1);

                    string aiResponse = null;
                    foreach (var message in messages)
                    {
                        if (message is TextMessage textMessage && textMessage.Role == Role.Assistant)
                        {
                            aiResponse = textMessage.Content;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(aiResponse))
                    {
                        return new CompanyNumberResult { IsSuccessful = false, ErrorMessage = "No response from AI" };
                    }

                    // Parse the AI response to extract company number
                    var companyNumber = ParseCompanyNumberResponse(aiResponse);
                    
                    return new CompanyNumberResult 
                    { 
                        IsSuccessful = true, 
                        CompanyNumber = companyNumber,
                        RawResponse = aiResponse
                    };
                }
                catch (Exception ex) when (ex.Message.Contains("rate limit") || ex.Message.Contains("429"))
                {
                    currentRetry++;
                    if (currentRetry >= maxRetries)
                    {
                        Console.WriteLine($"Rate limit exceeded after {maxRetries} retries. Returning empty result.");
                        return new CompanyNumberResult { IsSuccessful = false, ErrorMessage = "Rate limit exceeded" };
                    }
                    
                    int delayMs = (int)Math.Pow(2, currentRetry) * _config.RetryDelayMs;
                    Console.WriteLine($"Rate limit hit. Retrying in {delayMs/1000} seconds... (Attempt {currentRetry}/{maxRetries})");
                    await Task.Delay(delayMs);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                    return new CompanyNumberResult { IsSuccessful = false, ErrorMessage = ex.Message };
                }
            }

            return new CompanyNumberResult { IsSuccessful = false, ErrorMessage = "Max retries exceeded" };
        }

        private string ParseCompanyNumberResponse(string aiResponse)
        {
            try
            {
                // Clean the response
                var cleanResponse = aiResponse.Trim().Trim('"', '\'', '`');
                
                // Look for UK company number patterns (8 digits or 2 letters + 6 digits)
                var patterns = new[]
                {
                    @"\b\d{8}\b", // 8 digits
                    @"\b[A-Z]{2}\d{6}\b", // 2 letters + 6 digits
                    @"\b[A-Z]{2}\d{5}\b", // 2 letters + 5 digits (older format)
                    @"\b[A-Z]{2}\d{4}\b"  // 2 letters + 4 digits (very old format)
                };

                foreach (var pattern in patterns)
                {
                    var match = System.Text.RegularExpressions.Regex.Match(cleanResponse, pattern);
                    if (match.Success)
                    {
                        return match.Value.ToUpper();
                    }
                }

                // If no pattern found, try to extract any sequence that looks like a company number
                var words = cleanResponse.Split(new[] { ' ', '\n', '\t', ',', '.', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in words)
                {
                    var cleanWord = word.Trim().ToUpper();
                    if (cleanWord.Length >= 6 && cleanWord.Length <= 8)
                    {
                        // Check if it contains only digits or letters+digits
                        if (System.Text.RegularExpressions.Regex.IsMatch(cleanWord, @"^[A-Z0-9]+$"))
                        {
                            return cleanWord;
                        }
                    }
                }

                return cleanResponse; // Return the cleaned response if no pattern found
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing company number response: {ex.Message}");
                return aiResponse?.Trim() ?? string.Empty;
            }
        }
    }

    public class CompanyNumberResult
    {
        public bool IsSuccessful { get; set; }
        public string CompanyNumber { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string RawResponse { get; set; } = string.Empty;
    }
}
