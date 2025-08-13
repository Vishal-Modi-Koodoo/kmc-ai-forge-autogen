using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using KMC_Forge_BTL_API.Hubs;
using KMC_Forge_BTL_API.Contracts;

namespace KMC_Forge_BTL_API.Services
{
    public class SignalRService : ISignalRService
    {
        private readonly IHubContext<PortfolioHub> _hubContext;
        private readonly ILogger<SignalRService> _logger;

        public SignalRService(IHubContext<PortfolioHub> hubContext, ILogger<SignalRService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendFileUploadStatusAsync(string portfolioId, string status, string message)
        {
            try
            {
                await _hubContext.Clients.Group(portfolioId).SendAsync("ReceiveFileUploadStatus", status, message);
                _logger.LogInformation($"Sent file upload status to portfolio {portfolioId}: {status} - {message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending file upload status to portfolio {portfolioId}");
            }
        }

        public async Task SendValidationProgressAsync(string portfolioId, int currentStep, int totalSteps, string message)
        {
            try
            {
                await _hubContext.Clients.Group(portfolioId).SendAsync("ReceiveValidationProgress", currentStep, totalSteps, message);
                _logger.LogInformation($"Sent validation progress to portfolio {portfolioId}: {currentStep}/{totalSteps} - {message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending validation progress to portfolio {portfolioId}");
            }
        }

        public async Task SendValidationCompleteAsync(string portfolioId, object result)
        {
            try
            {
                await _hubContext.Clients.Group(portfolioId).SendAsync("ReceiveValidationComplete", result);
                _logger.LogInformation($"Sent validation complete to portfolio {portfolioId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending validation complete to portfolio {portfolioId}");
            }
        }

        public async Task SendValidationErrorAsync(string portfolioId, string errorMessage)
        {
            try
            {
                await _hubContext.Clients.Group(portfolioId).SendAsync("ReceiveValidationError", errorMessage);
                _logger.LogInformation($"Sent validation error to portfolio {portfolioId}: {errorMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending validation error to portfolio {portfolioId}");
            }
            }

        public async Task SendPortfolioUpdateAsync(string portfolioId, object update)
        {
            try
            {
                await _hubContext.Clients.Group(portfolioId).SendAsync("ReceivePortfolioUpdate", update);
                _logger.LogInformation($"Sent portfolio update to portfolio {portfolioId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending portfolio update to portfolio {portfolioId}");
            }
        }

        public async Task SendToUserAsync(string connectionId, string method, params object[] args)
        {
            try
            {
                await _hubContext.Clients.Client(connectionId).SendAsync(method, args);
                _logger.LogInformation($"Sent message to user {connectionId}: {method}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending message to user {connectionId}");
            }
        }

        public async Task SendToGroupAsync(string groupName, string method, params object[] args)
        {
            try
            {
                await _hubContext.Clients.Group(groupName).SendAsync(method, args);
                _logger.LogInformation($"Sent message to group {groupName}: {method}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending message to group {groupName}");
            }
        }

        public async Task SendToAllAsync(string method, params object[] args)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync(method, args);
                _logger.LogInformation($"Sent message to all clients: {method}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending message to all clients");
            }
        }
    }
} 
