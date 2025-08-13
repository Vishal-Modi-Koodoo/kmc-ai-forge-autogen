using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using KMC_Forge_BTL_Models.PDFExtractorResponse;

namespace KMC_Forge_BTL_Models.DBModels
{
    public class PropertyInfoCollection
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("propertyAddress")]
        public string PropertyAddress { get; set; }

        [BsonElement("propertyType")]
        public string PropertyType { get; set; }

        [BsonElement("yearPurchased")]
        public string YearPurchased { get; set; }

        [BsonElement("currentEstimatedValue")]
        public decimal CurrentEstimatedValue { get; set; }

        [BsonElement("rentalIncomePerMonth")]
        public decimal RentalIncomePerMonth { get; set; }

        [BsonElement("mortgagePaymentPerMonth")]
        public decimal MortgagePaymentPerMonth { get; set; }

        [BsonElement("owner")]
        public string Owner { get; set; }

        [BsonElement("lender")]
        public string Lender { get; set; }

        [BsonElement("dateOfMortgage")]
        public string DateOfMortgage { get; set; }

        [BsonElement("mortgageBalanceOutstanding")]
        public decimal MortgageBalanceOutstanding { get; set; }

        [BsonElement("annualServiceCharge")]
        public decimal AnnualServiceCharge { get; set; }

        [BsonElement("annualGroundRent")]
        public decimal AnnualGroundRent { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("companyInfoId")]
        public string CompanyInfoId { get; set; }

        [BsonElement("portfolioId")]
        public string PortfolioId { get; set; }

        [BsonElement("documentId")]
        public string DocumentId { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "Active";
    }
}
