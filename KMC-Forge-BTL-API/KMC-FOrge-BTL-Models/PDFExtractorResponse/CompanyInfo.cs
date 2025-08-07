using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace KMC_FOrge_BTL_Models.PDFExtractorResponse
{
    public class CompanyInfo
    {
        [JsonPropertyName("company_name")]
        public string CompanyName { get; set; }
        [JsonPropertyName("properties")]
        public List<PropertyInfo> Properties { get; set; } = new List<PropertyInfo>();
    }
}
