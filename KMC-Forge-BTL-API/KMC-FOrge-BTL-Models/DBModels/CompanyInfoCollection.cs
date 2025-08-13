using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using KMC_Forge_BTL_Models.PDFExtractorResponse;

namespace KMC_Forge_BTL_Models.DBModels
{
    public class CompanyInfoCollection
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("companyName")]
        public string CompanyName { get; set; }

        [BsonElement("properties")]
        public List<PropertyInfoCollection> Properties { get; set; } = new List<PropertyInfoCollection>();

        [BsonElement("documentId")]
        public string DocumentId { get; set; }
    }
}
