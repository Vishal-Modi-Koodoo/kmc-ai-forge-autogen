namespace KMC_AI_Forge_BTL_Agent.Models;

public class ValidationDecisionMessage
{
    public required string PortfolioId { get; set; }
    public DocumentAnalysisResult? DocumentAnalysis { get; set; }
    public FinancialVerificationResult? FinancialAnalysis { get; set; }
    public required string CorrelationId { get; set; }
} 