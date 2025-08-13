namespace KMC_Forge_BTL_Models.ImageDataExtractorResponse
{
    public class ImageExtractionResult
    {
        public string PersonsEntitled { get; set; } = string.Empty;
        public string BriefDescription { get; set; } = string.Empty;
    }

    public class OpenAIResponse
    {
        public Choice[] choices { get; set; }
    }

    public class Choice
    {
        public Message message { get; set; }
    }

    public class Message
    {
        public string content { get; set; }
    }
}
