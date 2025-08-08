using KMC_AI_Forge_BTL_Agent.Contracts;
using KMC_AI_Forge_BTL_Agent.Models;

namespace KMC_AI_Forge_BTL_Agent.Services;

public class PortfolioValidationService : IPortfolioValidationService
{
    private readonly IAgentRuntime _agentRuntime;
    private readonly ILogger<PortfolioValidationService> _logger;

    public PortfolioValidationService(IAgentRuntime agentRuntime, ILogger<PortfolioValidationService> logger)
    {
        _agentRuntime = agentRuntime;
        _logger = logger;
    }

    public async Task<ValidationResult> StartValidation(string portfolioId, List<UploadedDocument> documents)
    {
        var correlationId = Guid.NewGuid().ToString();
        
        _logger.LogInformation("Starting validation workflow for portfolio {PortfolioId}", portfolioId);

        try
        {
            // Phase 1: Document Processing (Document Intelligence Agent)
            var documentProcessingMessage = new DocumentProcessingMessage
            {
                PortfolioId = portfolioId,
                Documents = documents,
                CorrelationId = correlationId
            };

            // Phase 2: Financial Verification + Charges Compliance (Financial Verification Agent)
            var financialVerificationMessage = new FinancialVerificationMessage
            {
                PortfolioId = portfolioId,
                Documents = documents,
                CorrelationId = correlationId
            };

            // Publish messages to agents
            var documentTask = _agentRuntime.PublishMessage(
                documentProcessingMessage, 
                new TopicId("document_intelligence")
            );

            var financialTask = _agentRuntime.PublishMessage(
                financialVerificationMessage, 
                new TopicId("financial_verification")
            );

            // Wait for both processing agents to complete
            var documentResult = await documentTask as DocumentAnalysisResult;
            var financialResult = await financialTask as FinancialVerificationResult;

            // Phase 3: Orchestrator Decision (Lead Portfolio Validator)
            var orchestratorMessage = new ValidationDecisionMessage
            {
                PortfolioId = portfolioId,
                DocumentAnalysis = documentResult,
                FinancialAnalysis = financialResult,
                CorrelationId = correlationId
            };

            var finalDecision = await _agentRuntime.PublishMessage(
                orchestratorMessage, 
                new TopicId("lead_validator")
            ) as ValidationDecision;

            // Create comprehensive result package
            var validationResult = new ValidationResult
            {
                ValidationId = correlationId,
                PortfolioId = portfolioId,
                DocumentAnalysis = documentResult,
                FinancialAnalysis = financialResult,
                ChargesCompliance = financialResult?.ChargesCompliance,
                FinalDecision = finalDecision,
                ProcessingTime = documents.Any() ? DateTime.UtcNow - documents.First().UploadTimestamp : TimeSpan.Zero,
                ValidationTimestamp = DateTimeOffset.UtcNow,
                Status = "Completed"
            };

            _logger.LogInformation("Validation completed for portfolio {PortfolioId} with decision {Decision}", 
                portfolioId, finalDecision?.OverallDecision ?? "Unknown");

            return validationResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Validation failed for portfolio {PortfolioId}", portfolioId);
            throw;
        }
    }
}