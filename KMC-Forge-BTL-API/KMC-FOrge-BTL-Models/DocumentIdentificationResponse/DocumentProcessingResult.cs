using KMC_Forge_BTL_Models.Enums;
using KMC_Forge_BTL_Models.PDFExtractorResponse;
using KMC_Forge_BTL_Models.ImageDataExtractorResponse;

namespace KMC_Forge_BTL_Models.DocumentIdentificationResponse
{
    /// <summary>
    /// Result model for document processing operations
    /// </summary>
    public class DocumentProcessingResult
    {
        /// <summary>
        /// Whether the document is valid for processing
        /// </summary>
        public bool IsValid { get; set; }
        
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
        /// PDF extraction data for portfolio forms
        /// </summary>
        public CompanyInfo? PdfData { get; set; }
        
        /// <summary>
        /// List of image extraction results
        /// </summary>
        public List<ImageExtractionResult> ImageDataList { get; set; } = new List<ImageExtractionResult>();
        
        /// <summary>
        /// Extracted company number
        /// </summary>
        public string CompanyNumber { get; set; } = "";
        
        /// <summary>
        /// File path of the processed document
        /// </summary>
        public string FilePath { get; set; } = "";
        
        /// <summary>
        /// Name of the processed file
        /// </summary>
        public string FileName { get; set; } = "";
        
        /// <summary>
        /// Size of the processed file in bytes
        /// </summary>
        public long FileSize { get; set; }
        
        /// <summary>
        /// Processing message or status
        /// </summary>
        public string ProcessingMessage { get; set; } = "";

        public DocumentIdentificationProcessResult IdentificationResult { get; set; } = new DocumentIdentificationProcessResult();
    }
}
