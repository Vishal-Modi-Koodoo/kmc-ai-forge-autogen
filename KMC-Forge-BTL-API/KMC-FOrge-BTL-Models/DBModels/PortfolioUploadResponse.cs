using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using KMC_Forge_BTL_Models;
using KMC_Forge_BTL_Models.PDFExtractorResponse;
using KMC_Forge_BTL_Models.ImageDataExtractorResponse;

namespace KMC_Forge_BTL_Models.DBModels
{
    public class PortfolioUploadResponse
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("portfolioId")]
        public string PortfolioId { get; set; }

        [BsonElement("estimatedProcessingTime")]
        public string EstimatedProcessingTime { get; set; }

        [BsonElement("status")]
        public string Status { get; set; }


        [BsonElement("portfolioData")]
        public CompanyInfoCollection PortfolioData { get; set; }

        [BsonElement("chargesData")]
        public List<ImageExtractionResultCollection> ChargesData { get; set; }

        [BsonElement("uploadedDocuments")]
        public List<UploadedDocumentCollection> UploadedDocuments { get; set; }

        [BsonElement("invalidDocuments")]
        public List<InvalidDocumentInfoCollection> InvalidDocuments { get; set; }

        [BsonElement("summary")]
        public UploadSummary Summary { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class UploadSummary
    {
        [BsonElement("totalDocuments")]
        public int TotalDocuments { get; set; }

        [BsonElement("validDocuments")]
        public int ValidDocuments { get; set; }

        [BsonElement("invalidDocuments")]
        public int InvalidDocuments { get; set; }

        [BsonElement("processingCompleted")]
        public bool ProcessingCompleted { get; set; }
    }
}
