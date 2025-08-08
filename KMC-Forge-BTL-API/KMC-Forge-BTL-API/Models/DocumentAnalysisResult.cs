namespace KMC_AI_Forge_BTL_Agent.Models;

public class DocumentAnalysisResult
{
    public required string PortfolioId { get; set; }
    public string Status { get; set; } = "Completed";
    public List<string> ExtractedData { get; set; } = new();
    public List<string> ValidationMessages { get; set; } = new();
    public bool IsValid { get; set; } = false;
    public DateTimeOffset AnalysisTimestamp { get; set; } = DateTimeOffset.UtcNow;
} 