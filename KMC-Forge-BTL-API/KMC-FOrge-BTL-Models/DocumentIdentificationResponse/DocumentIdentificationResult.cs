using KMC_Forge_BTL_Models.Enums;

namespace KMC_Forge_BTL_Models.DocumentIdentificationResponse
{
    /// <summary>
    /// Response model for document type identification
    /// </summary>
    public class DocumentIdentificationResult
    {
        /// <summary>
        /// The identified document type
        /// </summary>
        public DocumentType DocumentType { get; set; }
        
        /// <summary>
        /// Confidence score for the identification (0.0 to 1.0)
        /// </summary>
        public double Confidence { get; set; }
        
        /// <summary>
        /// Additional reasoning or explanation for the identification
        /// </summary>
        public string? Reasoning { get; set; }
        
        /// <summary>
        /// Any additional metadata extracted during identification
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
        
        /// <summary>
        /// Whether the identification was successful
        /// </summary>
        public bool IsSuccessful { get; set; }
        
        /// <summary>
        /// Error message if identification failed
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}
