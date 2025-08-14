using Microsoft.AspNetCore.SignalR;
using KMC_Forge_BTL_API.Hubs;
using KMC_Forge_BTL_API.Enums;

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

        /// <summary>
        /// Send next step progress update
        /// </summary>
        public async Task SendNextStepProgressUpdateAsync(string portfolioId, ProcessingStep nextStep, string message = null)
        {
            try
            {
                var progressUpdate = new ProcessingUpdate
                {
                    PortfolioId = portfolioId,
                    Status = nextStep.ToString(),
                    Message = message ?? $"Starting {nextStep}...",
                    ProcessingStatus = ProcessingStatus.InProgress,
                    ProcessingStep = nextStep,
                    Progress = GetProgressPercentage(nextStep)
                };

                await _hubContext.Clients.Group($"portfolio_{portfolioId}").SendAsync("ProcessingUpdate", progressUpdate);
                _logger.LogInformation("Sent next step progress update for portfolio {PortfolioId}, next step: {NextStep}", portfolioId, nextStep);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending next step progress update for portfolio {PortfolioId}, next step: {NextStep}", portfolioId, nextStep);
            }
        }

        /// <summary>
        /// Send step completion update with correct progress percentage
        /// </summary>
        public async Task SendStepCompletionUpdateAsync(string portfolioId, ProcessingStep completedStep, string message = null)
        {
            try
            {
                var completionUpdate = new ProcessingUpdate
                {
                    PortfolioId = portfolioId,
                    Status = completedStep.ToString(),
                    Message = message ?? $"{completedStep} completed successfully.",
                    ProcessingStatus = ProcessingStatus.Success,
                    ProcessingStep = completedStep,
                    Progress = GetCompletionPercentage(completedStep)
                };

                await _hubContext.Clients.Group($"portfolio_{portfolioId}").SendAsync("ProcessingUpdate", completionUpdate);
                _logger.LogInformation("Sent step completion update for portfolio {PortfolioId}, completed step: {CompletedStep}", portfolioId, completedStep);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending step completion update for portfolio {PortfolioId}, completed step: {CompletedStep}", portfolioId, completedStep);
            }
        }

        /// <summary>
        /// Get progress percentage based on processing step
        /// </summary>
        private int GetProgressPercentage(ProcessingStep step)
        {
            return step switch
            {
                ProcessingStep.DocumentValidation => 0,
                ProcessingStep.PortfolioCompletion => 25,
                ProcessingStep.CompanyHouseValidation => 50,
                ProcessingStep.ProcessingComplete => 75,
                _ => 100
            };
        }

        /// <summary>
        /// Get completion percentage for when a step finishes
        /// </summary>
        private int GetCompletionPercentage(ProcessingStep step)
        {
            return step switch
            {
                ProcessingStep.DocumentValidation => 25,
                ProcessingStep.PortfolioCompletion => 50,
                ProcessingStep.CompanyHouseValidation => 75,
                ProcessingStep.ProcessingComplete => 100,
                _ => 100
            };
        }
    }
}
