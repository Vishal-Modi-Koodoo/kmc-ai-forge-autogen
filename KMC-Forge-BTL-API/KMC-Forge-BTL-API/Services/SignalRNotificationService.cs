using Microsoft.AspNetCore.SignalR;
using KMC_Forge_BTL_API.Hubs;

namespace KMC_Forge_BTL_API.Services
{
    /// <summary>
    /// Implementation of SignalR notification service
    /// </summary>
    public class SignalRNotificationService : ISignalRNotificationService
    {
        private readonly IHubContext<DocumentProcessingHub> _hubContext;
        private readonly ILogger<SignalRNotificationService> _logger;

        public SignalRNotificationService(IHubContext<DocumentProcessingHub> hubContext, ILogger<SignalRNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// Send document validation update
        /// </summary>
        public async Task SendDocumentValidationUpdateAsync(string portfolioId, DocumentValidationUpdate update)
        {
            try
            {
                update.PortfolioId = portfolioId;
                update.Status = "DocumentValidation";
                await _hubContext.Clients.Group($"portfolio_{portfolioId}").SendAsync("DocumentValidationUpdate", update);
                _logger.LogInformation("Sent document validation update for portfolio {PortfolioId}", portfolioId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending document validation update for portfolio {PortfolioId}", portfolioId);
            }
        }

        /// <summary>
        /// Send portfolio completion update    
        /// </summary>
        public async Task SendPortfolioCompletionUpdateAsync(string portfolioId, PortfolioCompletionUpdate update)
        {
            try
            {
                update.PortfolioId = portfolioId;
                update.Status = "PortfolioCompletion";
                await _hubContext.Clients.Group($"portfolio_{portfolioId}").SendAsync("PortfolioCompletionUpdate", update);
                _logger.LogInformation("Sent portfolio completion update for portfolio {PortfolioId}", portfolioId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending portfolio completion update for portfolio {PortfolioId}", portfolioId);
            }
        }

        /// <summary>
        /// Send company house validation update
        /// </summary>
        public async Task SendCompanyHouseValidationUpdateAsync(string portfolioId, CompanyHouseValidationUpdate update)
        {
            try
            {
                update.PortfolioId = portfolioId;
                update.Status = "CompanyHouseValidation";
                await _hubContext.Clients.Group($"portfolio_{portfolioId}").SendAsync("CompanyHouseValidationUpdate", update);
                _logger.LogInformation("Sent company house validation update for portfolio {PortfolioId}", portfolioId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending company house validation update for portfolio {PortfolioId}", portfolioId);
            }
        }

        /// <summary>
        /// Send processing complete update
        /// </summary>
        public async Task SendProcessingCompleteUpdateAsync(string portfolioId, ProcessingCompleteUpdate update)
        {
            try
            {
                update.PortfolioId = portfolioId;
                update.Status = "ProcessingComplete";
                update.Progress = 100;
                await _hubContext.Clients.Group($"portfolio_{portfolioId}").SendAsync("ProcessingCompleteUpdate", update);
                _logger.LogInformation("Sent processing complete update for portfolio {PortfolioId}", portfolioId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending processing complete update for portfolio {PortfolioId}", portfolioId);
            }
        }

        /// <summary>
        /// Send general processing update
        /// </summary>
        public async Task SendProcessingUpdateAsync(string portfolioId, ProcessingUpdate update)
        {
            try
            {
                update.PortfolioId = portfolioId;
                await _hubContext.Clients.Group($"portfolio_{portfolioId}").SendAsync("ProcessingUpdate", update);
                _logger.LogInformation("Sent general processing update for portfolio {PortfolioId}", portfolioId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending general processing update for portfolio {PortfolioId}", portfolioId);
            }
        }
    }
}
