using System.Text.Json;
using AutoGen;
using AutoGen.Core;
using AutoGen.OpenAI;
using KMC_Forge_BTL_Configurations;
using KMC_Forge_BTL_Core_Agent.Agents;

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

        public async Task<string> ValidatePortfolioFormAsync(DocumentProcessingResult processingResult)
        {
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
                        message: JsonSerializer.Serialize(processingResult.PdfData, new JsonSerializerOptions { WriteIndented = true }),
                        maxRound: 1);

                    string aiJson = null;
                    foreach (var message in messages)
                    {
                        if (message is TextMessage textMessage && textMessage.Role == Role.Assistant)
                        {
                            aiJson = textMessage.Content;
                            break; // Get only the first assistant response to prevent loops
                        }
                    }

                    if (!string.IsNullOrEmpty(aiJson))
                    {
                        Console.WriteLine($"AI Response: {aiJson}");
                        
                        // Try to parse the validation result
                        try
                        {
                            var validationResult = JsonSerializer.Deserialize<JsonElement>(aiJson);
                            
                            // Check if we have a validation summary
                            if (validationResult.TryGetProperty("validation_summary", out var summaryElement))
                            {
                                var totalOriginal = summaryElement.TryGetProperty("total_properties_original", out var totalProp) ? totalProp.GetInt32() : 0;
                                var validProps = summaryElement.TryGetProperty("valid_properties", out var validProp) ? validProp.GetInt32() : 0;
                                var excludedProps = summaryElement.TryGetProperty("excluded_properties", out var excludedProp) ? excludedProp.GetInt32() : 0;
                                
                                Console.WriteLine($"Portfolio validation completed: {validProps} valid properties, {excludedProps} excluded properties");
                                
                                // Log exclusion reasons if available
                                if (summaryElement.TryGetProperty("exclusion_reasons", out var exclusionReasons))
                                {
                                    foreach (var reason in exclusionReasons.EnumerateArray())
                                    {
                                        var propertyIndex = reason.TryGetProperty("property_index", out var idx) ? idx.GetInt32() : -1;
                                        var propertyAddress = reason.TryGetProperty("property_address", out var addr) ? addr.GetString() : "Unknown Address";
                                        var missingFields = reason.TryGetProperty("missing_fields", out var missing) ? missing.EnumerateArray().Select(f => f.GetString()).ToArray() : new string[0];
                                        var dateErrors = reason.TryGetProperty("date_validation_errors", out var dateErr) ? dateErr.EnumerateArray().Select(f => f.GetString()).ToArray() : new string[0];
                                        var financialErrors = reason.TryGetProperty("financial_validation_errors", out var financialErr) ? financialErr.EnumerateArray().Select(f => f.GetString()).ToArray() : new string[0];
                                        var exclusionSummary = reason.TryGetProperty("exclusion_summary", out var summary) ? summary.GetString() : "No summary provided";
                                        
                                        Console.WriteLine($"Property {propertyIndex} ({propertyAddress}) excluded - {exclusionSummary}");
                                        
                                        if (missingFields.Length > 0)
                                        {
                                            Console.WriteLine($"  Missing fields: {string.Join(", ", missingFields)}");
                                        }
                                        if (dateErrors.Length > 0)
                                        {
                                            Console.WriteLine($"  Date validation errors: {string.Join(", ", dateErrors)}");
                                        }
                                        if (financialErrors.Length > 0)
                                        {
                                            Console.WriteLine($"  Financial validation errors: {string.Join(", ", financialErrors)}");
                                        }
                                    }
                                }
                                
                                // Check if we have a cleaned portfolio
                                if (validationResult.TryGetProperty("cleaned_portfolio", out var cleanedElement))
                                {
                                    var statusMessage = $"Portfolio validation completed: {validProps} valid properties, {excludedProps} excluded properties\nPortfolio cleaned successfully";
                                    Console.WriteLine(statusMessage);
                                    return statusMessage;
                                }
                            }
                        }
                        catch (JsonException ex)
                        {
                            Console.WriteLine($"Failed to parse AI response as JSON: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No response received from AI agent");
                    }
                    
                    // If we get here, validation failed or couldn't parse response
                    // Break out of the retry loop since we got a response (even if invalid)
                    break;
                }
                catch (Exception ex) when (ex.Message.Contains("rate limit") || ex.Message.Contains("429"))
                {
                    currentRetry++;
                    if (currentRetry >= maxRetries)
                    {
                        var errorMessage = $"Rate limit exceeded after {maxRetries} retries. Portfolio validation failed.";
                        Console.WriteLine(errorMessage);
                        return errorMessage;
                    }
                    
                    int delayMs = (int)Math.Pow(2, currentRetry) * _config.RetryDelayMs; // Use configured delay
                    Console.WriteLine($"Rate limit hit. Retrying in {delayMs/1000} seconds... (Attempt {currentRetry}/{maxRetries})");
                    await Task.Delay(delayMs);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                    // Break out of the loop on unexpected errors
                    break;
                }
            }

            // If we get here, all retries failed
            return "Portfolio validation failed after all retry attempts.";
        }

    }
}