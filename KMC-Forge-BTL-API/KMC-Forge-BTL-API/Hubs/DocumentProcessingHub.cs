using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using KMC_Forge_BTL_API.Enums;

namespace KMC_Forge_BTL_API.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time document processing updates
    /// </summary>
    public class DocumentProcessingHub : Hub
    {
        /// <summary>
        /// Called when a client connects to the hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await Clients.Caller.SendAsync("Connected", "Successfully connected to Document Processing Hub");
        }

        /// <summary>
        /// Called when a client disconnects from the hub
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Join a specific portfolio processing group
        /// </summary>
        public async Task JoinPortfolioGroup(string portfolioId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"portfolio_{portfolioId}");
            await Clients.Caller.SendAsync("JoinedGroup", $"Joined portfolio group: {portfolioId}");
        }

        /// <summary>
        /// Leave a specific portfolio processing group
        /// </summary>
        public async Task LeavePortfolioGroup(string portfolioId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"portfolio_{portfolioId}");
            await Clients.Caller.SendAsync("LeftGroup", $"Left portfolio group: {portfolioId}");
        }
    }

    /// <summary>
    /// Models for SignalR messages
    /// </summary>
    public class ProcessingUpdate
    {
        public string PortfolioId { get; set; } = "";
        public string Status { get; set; } = "";
        public string Message { get; set; } = "";
        public int Progress { get; set; } = 0;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public object? Data { get; set; }
        public ProcessingStatus ProcessingStatus { get; set; } = ProcessingStatus.Success;
        public ProcessingStep ProcessingStep { get; set; } = ProcessingStep.DocumentValidation;
    }

    public class DocumentValidationUpdate : ProcessingUpdate
    {
        public int TotalDocuments { get; set; }
        public int ValidDocuments { get; set; }
        public int InvalidDocuments { get; set; }
        public List<string> ValidFileNames { get; set; } = new List<string>();
        public List<string> InvalidFileNames { get; set; } = new List<string>();
    }

    public class PortfolioCompletionUpdate : ProcessingUpdate
    {
        public bool HasPortfolioData { get; set; }
        public string? CompanyName { get; set; }
        public int PropertyCount { get; set; }
    }

    public class CompanyHouseValidationUpdate : ProcessingUpdate
    {
        public bool HasCompanyData { get; set; }
        public string? CompanyNumber { get; set; }
        public int ChargeCount { get; set; }
    }

    public class ProcessingCompleteUpdate : ProcessingUpdate
    {
        public string ProcessingTime { get; set; } = "";
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
