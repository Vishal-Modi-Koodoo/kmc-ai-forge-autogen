namespace KMC_AI_Forge_BTL_Agent.Models;

public class ValidationResult
{
    public required string ValidationId { get; set; }
    public required string PortfolioId { get; set; }
    public DocumentAnalysisResult? DocumentAnalysis { get; set; }
    public FinancialVerificationResult? FinancialAnalysis { get; set; }
    public string? ChargesCompliance { get; set; }
    public ValidationDecision? FinalDecision { get; set; }
    public TimeSpan? ProcessingTime { get; set; }
    public DateTimeOffset ValidationTimestamp { get; set; } = DateTimeOffset.UtcNow;
    public string Status { get; set; } = "Pending";
    public string? EstimatedCompletionTime { get; set; }
} 