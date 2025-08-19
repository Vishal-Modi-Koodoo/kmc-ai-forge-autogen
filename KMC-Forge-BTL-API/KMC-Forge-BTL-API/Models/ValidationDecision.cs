namespace KMC.Forge.BTL.API.Models;

public class ValidationDecision
{
    public required string PortfolioId { get; set; }
    public string OverallDecision { get; set; } = "Pending";
    public string DecisionReason { get; set; } = "";
    public List<string> DecisionFactors { get; set; } = new();
    public DateTimeOffset DecisionTimestamp { get; set; } = DateTimeOffset.UtcNow;
    public string ValidatorId { get; set; } = "";
} 