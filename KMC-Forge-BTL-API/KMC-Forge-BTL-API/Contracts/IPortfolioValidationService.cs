using KMC_AI_Forge_BTL_Agent.Models;

namespace KMC_AI_Forge_BTL_Agent.Contracts;

public interface IPortfolioValidationService
{
    /// <summary>
    /// Starts the validation workflow for a portfolio
    /// </summary>
    /// <param name="portfolioId">The portfolio identifier</param>
    /// <param name="uploadedDocuments">The list of uploaded documents to validate</param>
    /// <returns>The validation result containing the validation ID</returns>
    Task<ValidationResult> StartValidation(string portfolioId, List<UploadedDocument> uploadedDocuments);
} 