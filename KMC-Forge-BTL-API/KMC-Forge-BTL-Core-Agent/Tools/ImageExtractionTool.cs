using AutoGen;
using AutoGen.Core;
using AutoGen.OpenAI;
using KMC_FOrge_BTL_Models.ImageDataExtractorResponse;

namespace KMC_Forge_BTL_Core_Agent.Tools
{
    public class ImageExtractionTool
    {
        private readonly MiddlewareStreamingAgent<OpenAIChatAgent> _imageAnalyserAgent;

        // Constructor to initialize the PDFAnalyserAgent
        public ImageExtractionTool(MiddlewareStreamingAgent<OpenAIChatAgent> imageAnalyserAgent)
        {
            _imageAnalyserAgent = imageAnalyserAgent;
        }

        public async Task<ChargeInfo> ExtractDataAsync(string path)
        {
            
            try
            {
                using var httpClient = new HttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(path);


                var userProxy = new UserProxyAgent(
                    name: "user",
                    systemMessage: "You are an expert at extracting structured information from images. Given an image, extract only the following fields as JSON: { \"PersonsEntitled\": \"...\", \"BriefDescription\": \"...\" }. Do not return anything else.",
                    defaultReply: "Thank you for the output",
                    humanInputMode: HumanInputMode.NEVER
                ).RegisterPrintMessage();

                Console.WriteLine("\nAnalyzing image content with Azure OpenAI LLM...\n");


                var messages = await userProxy.InitiateChatAsync(
                    receiver: _imageAnalyserAgent,
                    message: Convert.ToBase64String(imageBytes),
                    maxRound: 1
                );

                string aiJson = null;
                foreach (var message in messages)
                {
                    if (message is TextMessage textMessage && textMessage.Role == Role.Assistant)
                    {
                        aiJson = textMessage.Content;
                    }
                }

                if (string.IsNullOrWhiteSpace(aiJson))
                {
                    Console.WriteLine("No data could be extracted from the image.");
                    return new ChargeInfo();
                }

                using var doc = System.Text.Json.JsonDocument.Parse(aiJson);
                var root = doc.RootElement;

                var chargeInfo = new ChargeInfo
                {
                    PersonsEntitled = root.TryGetProperty("PersonsEntitled", out var personProp) ? personProp.GetString() : null,
                    BriefDescription = root.TryGetProperty("BriefDescription", out var descProp) ? descProp.GetString() : null
                };

                return chargeInfo;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error extracting data from image: " + ex.Message);
                return new ChargeInfo();
            }
        }

    }
}