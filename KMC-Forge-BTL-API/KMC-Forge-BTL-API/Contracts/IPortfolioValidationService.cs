using KMC.Forge.BTL.API.Models;

namespace KMC.Forge.BTL.API.Contracts;

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