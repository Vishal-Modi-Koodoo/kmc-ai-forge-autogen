using AutoGen;
using AutoGen.Core;
using AutoGen.OpenAI;
using Azure.AI.OpenAI;
using KMC_Forge_BTL_Configurations;
using KMC_FOrge_BTL_Models.ImageDataExtractorResponse;

namespace KMC_Forge_BTL_Core_Agent.Tools
{
    public class ImageExtractionTool : OpenAIChatAgent
    {
        private readonly AppConfiguration _config;

        public ImageExtractionTool(AzureOpenAIClient openAIClient, string model, string analysisPrompt)
            : base(
                name: "image_analyzer",
                systemMessage: analysisPrompt,
                chatClient: openAIClient.GetChatClient(model))
        {
            _config = AppConfiguration.Instance;
        }

        public async Task<ImageExtractionResult> ExtractDataAsync(string path)
        {
            string extractedText = ImageTextExtractor.ExtractTextFromImage(path);

            if (string.IsNullOrWhiteSpace(extractedText))
            {
                Console.WriteLine("No text could be extracted from the image.");
                return new ImageExtractionResult();
            }

            var userProxy = new UserProxyAgent(
                name: "user",
                systemMessage: "check the values",
                defaultReply: "Thank you for the output",
                humanInputMode: HumanInputMode.NEVER)
                .RegisterPrintMessage();

            Console.WriteLine("\nAnalyzing Image content with AI...\n");

            // Retry logic with exponential backoff using configuration
            int maxRetries = _config.MaxRetries;
            int currentRetry = 0;
            
            while (currentRetry < maxRetries)
            {
                try
                {
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

                    var imageExtractionResult = System.Text.Json.JsonSerializer.Deserialize<ImageExtractionResult>(aiJson);
                    return imageExtractionResult;
                }
                catch (Exception ex) when (ex.Message.Contains("rate limit") || ex.Message.Contains("429"))
                {
                    currentRetry++;
                    if (currentRetry >= maxRetries)
                    {
                        Console.WriteLine($"Rate limit exceeded after {maxRetries} retries. Returning empty result.");
                        return new ImageExtractionResult();
                    }
                    
                    int delayMs = (int)Math.Pow(2, currentRetry) * _config.RetryDelayMs; // Use configured delay
                    Console.WriteLine($"Rate limit hit. Retrying in {delayMs/1000} seconds... (Attempt {currentRetry}/{maxRetries})");
                    await Task.Delay(delayMs);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                    return new ImageExtractionResult();
                }
            }

            return new ImageExtractionResult();
        }
    }
}