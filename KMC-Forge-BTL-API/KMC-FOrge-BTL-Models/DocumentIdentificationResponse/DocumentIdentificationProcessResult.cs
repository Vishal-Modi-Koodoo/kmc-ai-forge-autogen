using KMC_Forge_BTL_Models.Enums;

namespace KMC_Forge_BTL_Models.DocumentIdentificationResponse
{
    /// <summary>
    /// Result model for document identification process
    /// </summary>
    public class DocumentIdentificationProcessResult
    {
        /// <summary>
        /// Whether the identification process was successful
        /// </summary>
        public bool IsSuccessful { get; set; }
        
        /// <summary>
        /// The identified document type
        /// </summary>
        public DocumentType DocumentType { get; set; }
        
        /// <summary>
        /// Confidence score for the identification (0.0 to 1.0)
        /// </summary>
        public double Confidence { get; set; }
        
        /// <summary>
        /// Extracted document content
        /// </summary>
        public string DocumentContent { get; set; } = "";
        
        /// <summary>
        /// Error message if identification failed
        /// </summary>
        public string ErrorMessage { get; set; } = "";
    }
}
