using KMC_Forge_BTL_Configurations;
using KMC_FOrge_BTL_Models.ImageDataExtractorResponse;
using System.Text;
using System.Text.Json;

namespace KMC_Forge_BTL_Core_Agent.Tools
{
    public class ImageExtractionTool 
    {
        private readonly AppConfiguration _config = AppConfiguration.Instance;
        private readonly HttpClient _httpClient;
        private readonly string _apiEndpoint;

        public ImageExtractionTool(string apiEndpoint = "https://api.openai.com/v1/chat/completions")
        {
            _httpClient = new HttpClient();
            _apiEndpoint = apiEndpoint;
            _config = AppConfiguration.Instance;
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.OpenAIApiKey}");
        }

        public async Task<ImageExtractionResult> ExtractDataAsync(string imagePath)
        {
            try
            {
                // Read image and convert to base64
                byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);
                string base64Image = Convert.ToBase64String(imageBytes);
                string mimeType = GetMimeType(imagePath);

                // Prepare the request
                var requestBody = new
                {
                    model = "gpt-4-vision-preview",
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = new object[]
                            {
                                new
                                {
                                    type = "text",
                                    text = "You are an expert at extracting structured information from images. Extract 'Persons entitled' and 'Brief description' from the image and return only JSON like: { \"PersonsEntitled\": \"...\", \"BriefDescription\": \"...\" }"
                                },
                                new
                                {
                                    type = "image_url",
                                    image_url = new
                                    {
                                        url = $"data:{mimeType};base64,{base64Image}"
                                    }
                                }
                            }
                        }
                    },
                    max_tokens = 300
                };

                string jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Send request
                var response = await _httpClient.PostAsync(_apiEndpoint, content);
                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);

                // Extract JSON from the response
                string extractedText = apiResponse.choices[0].message.content.Trim();

                // Parse the extracted JSON
                var result = JsonSerializer.Deserialize<ImageExtractionResult>(extractedText);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error extracting information from image: {ex.Message}", ex);
            }
        }

        private string GetMimeType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "image/jpeg"
            };
        }
    }
}