using System.Text.Json.Serialization;

namespace KMC_Forge_BTL_Models.PDFExtractorResponse
{
    public class PropertyInfo
    {
        [JsonPropertyName("property_address")]
        public string PropertyAddress { get; set; }
        [JsonPropertyName("property_type")]
        public string PropertyType { get; set; }
        [JsonPropertyName("year_purchased")]
        public string YearPurchased { get; set; }
        [JsonPropertyName("current_estimated_value")]
        public decimal CurrentEstimatedValue { get; set; }
        [JsonPropertyName("rental_income_per_month")]
        public decimal RentalIncomePerMonth { get; set; }
        [JsonPropertyName("mortgage_payment_per_month")]
        public decimal MortgagePaymentPerMonth { get; set; }
        [JsonPropertyName("owner")]
        public string Owner { get; set; }
        [JsonPropertyName("lender")]
        public string Lender { get; set; }
        [JsonPropertyName("date_of_mortgage")]
        public string DateOfMortgage { get; set; } // use DateTime if needed
        [JsonPropertyName("mortgage_balance_outstanding")]
        public decimal MortgageBalanceOutstanding { get; set; }
        [JsonPropertyName("annual_service_charge")]
        public decimal AnnualServiceCharge { get; set; }
        [JsonPropertyName("annual_ground_rent")]
        public decimal AnnualGroundRent { get; set; }
    }
}
