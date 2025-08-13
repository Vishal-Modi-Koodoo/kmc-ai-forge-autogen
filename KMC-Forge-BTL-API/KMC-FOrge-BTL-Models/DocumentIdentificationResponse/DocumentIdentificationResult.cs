using KMC_Forge_BTL_Models.Enums;

namespace KMC_Forge_BTL_Models.DocumentIdentificationResponse
{
    public class DocumentIdentificationResult
    {
        public DocumentType DocumentType { get; set; }
        public double Confidence { get; set; }
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
        public string? DocumentContent { get; set; }
        public string? Reasoning { get; set; }
        public DateTime IdentificationTimestamp { get; set; } = DateTime.UtcNow;
    }
}
