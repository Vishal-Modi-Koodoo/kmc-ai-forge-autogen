using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace KMC_Forge_BTL_API.Hubs
{
    public class PortfolioHub : Hub
    {
        private readonly ILogger<PortfolioHub> _logger;

        public PortfolioHub(ILogger<PortfolioHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinPortfolioGroup(string portfolioId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, portfolioId);
            _logger.LogInformation($"Client {Context.ConnectionId} joined portfolio group: {portfolioId}");
        }

        public async Task LeavePortfolioGroup(string portfolioId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, portfolioId);
            _logger.LogInformation($"Client {Context.ConnectionId} left portfolio group: {portfolioId}");
        }

        public async Task SendPortfolioUpdate(string portfolioId, object update)
        {
            await Clients.Group(portfolioId).SendAsync("ReceivePortfolioUpdate", update);
            _logger.LogInformation($"Sent portfolio update to group {portfolioId}");
        }

        public async Task SendValidationProgress(string portfolioId, int currentStep, int totalSteps, string message)
        {
            await Clients.Group(portfolioId).SendAsync("ReceiveValidationProgress", currentStep, totalSteps, message);
            _logger.LogInformation($"Sent validation progress to group {portfolioId}: {currentStep}/{totalSteps} - {message}");
        }

        public async Task SendValidationComplete(string portfolioId, object result)
        {
            await Clients.Group(portfolioId).SendAsync("ReceiveValidationComplete", result);
            _logger.LogInformation($"Sent validation complete to group {portfolioId}");
        }

        public async Task SendValidationError(string portfolioId, string errorMessage)
        {
            await Clients.Group(portfolioId).SendAsync("ReceiveValidationError", errorMessage);
            _logger.LogInformation($"Sent validation error to group {portfolioId}: {errorMessage}");
        }

        public async Task SendFileUploadStatus(string portfolioId, string status, string message)
        {
            await Clients.Group(portfolioId).SendAsync("ReceiveFileUploadStatus", status, message);
            _logger.LogInformation($"Sent file upload status to group {portfolioId}: {status} - {message}");
        }
    }
}
