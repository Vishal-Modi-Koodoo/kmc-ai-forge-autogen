using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace KMC_Forge_BTL_Models.DBModels
{
    public class UploadedDocumentCollection
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("documentId")]
        public string DocumentId { get; set; }

        [BsonElement("documentType")]
        public string DocumentType { get; set; }

        [BsonElement("fileName")]
        public string FileName { get; set; }

        [BsonElement("filePath")]
        public string FilePath { get; set; }

        [BsonElement("contentType")]
        public string ContentType { get; set; }

        [BsonElement("fileSize")]
        public long FileSize { get; set; }

        [BsonElement("uploadTimestamp")]
        public DateTimeOffset UploadTimestamp { get; set; } = DateTimeOffset.UtcNow;

        [BsonElement("portfolioId")]
        public string PortfolioId { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("status")]
        public string Status { get; set; } = "Active";
    }
}
