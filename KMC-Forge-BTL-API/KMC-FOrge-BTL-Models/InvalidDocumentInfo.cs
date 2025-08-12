namespace KMC_Forge_BTL_Models
{
    /// <summary>
    /// Information about an invalid document
    /// </summary>
    public class InvalidDocumentInfo
    {
        /// <summary>
        /// Name of the uploaded file
        /// </summary>
        public string FileName { get; set; } = string.Empty;
        
        /// <summary>
        /// Expected document type based on form field
        /// </summary>
        public string ExpectedType { get; set; } = string.Empty;
        
        /// <summary>
        /// Document type identified by AI (if available)
        /// </summary>
        public string? IdentifiedType { get; set; }
        
        /// <summary>
        /// Confidence score from AI identification (0.0-1.0)
        /// </summary>
        public double? Confidence { get; set; }
        
        /// <summary>
        /// Reason why the document was considered invalid
        /// </summary>
        public string Reason { get; set; } = string.Empty;
        
        /// <summary>
        /// Size of the uploaded file in bytes
        /// </summary>
        public long FileSize { get; set; }
        
        /// <summary>
        /// Path where the file was stored (for potential manual review)
        /// </summary>
        public string? FilePath { get; set; }
    }
}
