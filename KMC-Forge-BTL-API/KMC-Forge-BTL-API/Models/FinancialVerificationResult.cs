namespace KMC.Forge.BTL.API.Models;

public class FinancialVerificationResult
{
    public required string PortfolioId { get; set; }
    public string Status { get; set; } = "Completed";
    public string? ChargesCompliance { get; set; }
    public List<string> FinancialMetrics { get; set; } = new();
    public List<string> ValidationMessages { get; set; } = new();
    public bool IsValid { get; set; } = false;
    public DateTimeOffset VerificationTimestamp { get; set; } = DateTimeOffset.UtcNow;
} 