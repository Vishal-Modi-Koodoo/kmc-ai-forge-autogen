using KMC_Forge_BTL_Models.Enums;

namespace KMC_Forge_BTL_Models.DocumentIdentificationResponse
{
    public class DocumentIdentificationResponse
    {
        public bool Success { get; set; }
        public DocumentIdentificationResult Result { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public DateTime ResponseTimestamp { get; set; } = DateTime.UtcNow;
    }
}
