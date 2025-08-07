using KMC_AI_Forge_BTL_Agent.Models;

public class PortfolioUploadResponse
{
    public required string PortfolioId { get; set; }
    public required string ValidationId { get; set; }
    public required string EstimatedProcessingTime { get; set; }
    public required string Status { get; set; }
    public required List<UploadedDocument> UploadedDocuments { get; set; }
}