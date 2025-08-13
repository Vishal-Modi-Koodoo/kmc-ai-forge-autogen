using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using KMC_Forge_BTL_Models.ImageDataExtractorResponse;

namespace KMC_Forge_BTL_Models.DBModels
{
    public class ImageExtractionResultCollection
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("personsEntitled")]
        public string PersonsEntitled { get; set; } = string.Empty;

        [BsonElement("briefDescription")]
        public string BriefDescription { get; set; } = string.Empty;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("portfolioId")]
        public string PortfolioId { get; set; }

        [BsonElement("documentId")]
        public string DocumentId { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "Active";
    }
}
