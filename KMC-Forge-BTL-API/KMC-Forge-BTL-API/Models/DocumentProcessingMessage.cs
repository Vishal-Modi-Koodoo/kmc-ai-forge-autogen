using System.Collections.Generic;

namespace KMC_AI_Forge_BTL_Agent.Models;

public class DocumentProcessingMessage
{
    public required string PortfolioId { get; set; }
    public required List<UploadedDocument> Documents { get; set; }
    public required string CorrelationId { get; set; }
} 