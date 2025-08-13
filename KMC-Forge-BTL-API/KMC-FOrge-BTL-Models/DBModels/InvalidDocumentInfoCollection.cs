using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using KMC_Forge_BTL_Models;

namespace KMC_Forge_BTL_Models.DBModels
{
    public class InvalidDocumentInfoCollection
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("fileName")]
        public string FileName { get; set; } = string.Empty;

        [BsonElement("expectedType")]
        public string ExpectedType { get; set; } = string.Empty;

        [BsonElement("identifiedType")]
        public string? IdentifiedType { get; set; }

        [BsonElement("confidence")]
        public double? Confidence { get; set; }

        [BsonElement("reason")]
        public string Reason { get; set; } = string.Empty;

        [BsonElement("fileSize")]
        public long FileSize { get; set; }

        [BsonElement("filePath")]
        public string? FilePath { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("portfolioId")]
        public string PortfolioId { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "Active";
    }
}
