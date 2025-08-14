using KMC_Forge_BTL_API.Hubs;
using KMC_Forge_BTL_API.Enums;

namespace KMC_Forge_BTL_API.Services
{
    /// <summary>
    /// Interface for SignalR notification service
    /// </summary>
    public interface ISignalRNotificationService
    {
        /// <summary>
        /// Send document validation update
        /// </summary>
        Task SendDocumentValidationUpdateAsync(string portfolioId, DocumentValidationUpdate update);

        /// <summary>
        /// Send portfolio completion update
        /// </summary>
        Task SendPortfolioCompletionUpdateAsync(string portfolioId, PortfolioCompletionUpdate update);

        /// <summary>
        /// Send company house validation update
        /// </summary>
        Task SendCompanyHouseValidationUpdateAsync(string portfolioId, CompanyHouseValidationUpdate update);

        /// <summary>
        /// Send processing complete update
        /// </summary>
        Task SendProcessingCompleteUpdateAsync(string portfolioId, ProcessingCompleteUpdate update);

        /// <summary>
        /// Send general processing update
        /// </summary>
        Task SendProcessingUpdateAsync(string portfolioId, ProcessingUpdate update);

        /// <summary>
        /// Send next step progress update
        /// </summary>
        Task SendNextStepProgressUpdateAsync(string portfolioId, ProcessingStep nextStep, string message = null);

        /// <summary>
        /// Send step completion update with correct progress percentage
        /// </summary>
        Task SendStepCompletionUpdateAsync(string portfolioId, ProcessingStep completedStep, string message = null);
    }
}
