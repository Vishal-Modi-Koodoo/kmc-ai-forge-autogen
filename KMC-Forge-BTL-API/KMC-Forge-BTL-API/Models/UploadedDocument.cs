namespace KMC_AI_Forge_BTL_Agent.Models;

public class UploadedDocument
{
    public required string DocumentId { get; set; }
    public required string DocumentType { get; set; }
    public required string FileName { get; set; }
    public required string FilePath { get; set; }
    public required string ContentType { get; set; }
    public long FileSize { get; set; }
    public DateTimeOffset UploadTimestamp { get; set; } = DateTimeOffset.UtcNow;
    public required string PortfolioId { get; set; }
} 