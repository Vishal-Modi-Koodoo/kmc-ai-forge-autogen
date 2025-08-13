using System;

namespace KMC_Forge_BTL_Models.DBModels
{
    public class UploadedDocumentModel
    {
        public string DocumentId { get; set; }
        public string DocumentType { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public DateTimeOffset UploadTimestamp { get; set; } = DateTimeOffset.UtcNow;
        public string PortfolioId { get; set; }
    }
}
