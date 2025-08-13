using System.Threading.Tasks;

namespace KMC_Forge_BTL_API.Contracts
{
    public interface ISignalRService
    {
        Task SendFileUploadStatusAsync(string portfolioId, string status, string message);
        Task SendValidationProgressAsync(string portfolioId, int currentStep, int totalSteps, string message);
        Task SendValidationCompleteAsync(string portfolioId, object result);
        Task SendValidationErrorAsync(string portfolioId, string errorMessage);
        Task SendPortfolioUpdateAsync(string portfolioId, object update);
        Task SendToUserAsync(string connectionId, string method, params object[] args);
        Task SendToGroupAsync(string groupName, string method, params object[] args);
        Task SendToAllAsync(string method, params object[] args);
    }
}
