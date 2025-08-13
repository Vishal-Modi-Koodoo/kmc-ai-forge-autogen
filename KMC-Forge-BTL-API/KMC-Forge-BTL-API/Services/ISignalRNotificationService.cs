using KMC_Forge_BTL_API.Hubs;

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
    }
}
