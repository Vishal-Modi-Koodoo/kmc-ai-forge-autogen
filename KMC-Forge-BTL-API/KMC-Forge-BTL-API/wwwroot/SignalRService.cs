using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace KMC_Forge_BTL_API.Services
{
    public interface IBlazorSignalRService
    {
        Task ConnectAsync();
        Task DisconnectAsync();
        Task JoinPortfolioGroupAsync(string portfolioId);
        Task LeavePortfolioGroupAsync(string portfolioId);
        bool IsConnected { get; }
        event Action<string, string>? OnFileUploadStatus;
        event Action<int, int, string>? OnValidationProgress;
        event Action<object>? OnValidationComplete;
        event Action<string>? OnValidationError;
        event Action<object>? OnPortfolioUpdate;
        event Action<bool>? OnConnectionStateChanged;
    }

    public class BlazorSignalRService : IBlazorSignalRService, IAsyncDisposable
    {
        private readonly ILogger<BlazorSignalRService> _logger;
        private HubConnection? _hubConnection;
        private readonly string _hubUrl;

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public event Action<string, string>? OnFileUploadStatus;
        public event Action<int, int, string>? OnValidationProgress;
        public event Action<object>? OnValidationComplete;
        public event Action<string>? OnValidationError;
        public event Action<object>? OnPortfolioUpdate;
        public event Action<bool>? OnConnectionStateChanged;

        public BlazorSignalRService(ILogger<BlazorSignalRService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _hubUrl = configuration["SignalR:HubUrl"] ?? "https://localhost:5001/portfolioHub";
        }

        public async Task ConnectAsync()
        {
            try
            {
                if (_hubConnection != null)
                {
                    await _hubConnection.DisposeAsync();
                }

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_hubUrl)
                    .WithAutomaticReconnect()
                    .Build();

                // Register event handlers
                _hubConnection.On<string, string>("ReceiveFileUploadStatus", (status, message) =>
                {
                    _logger.LogInformation("Received file upload status: {Status} - {Message}", status, message);
                    OnFileUploadStatus?.Invoke(status, message);
                });

                _hubConnection.On<int, int, string>("ReceiveValidationProgress", (currentStep, totalSteps, message) =>
                {
                    _logger.LogInformation("Received validation progress: {CurrentStep}/{TotalSteps} - {Message}", currentStep, totalSteps, message);
                    OnValidationProgress?.Invoke(currentStep, totalSteps, message);
                });

                _hubConnection.On<object>("ReceiveValidationComplete", (result) =>
                {
                    var resultJson = JsonSerializer.Serialize(result);
                    _logger.LogInformation("Received validation complete: {Result}", resultJson);
                    OnValidationComplete?.Invoke(result);
                });

                _hubConnection.On<string>("ReceiveValidationError", (errorMessage) =>
                {
                    _logger.LogInformation("Received validation error: {ErrorMessage}", errorMessage);
                    OnValidationError?.Invoke(errorMessage);
                });

                _hubConnection.On<object>("ReceivePortfolioUpdate", (update) =>
                {
                    var updateJson = JsonSerializer.Serialize(update);
                    _logger.LogInformation("Received portfolio update: {Update}", updateJson);
                    OnPortfolioUpdate?.Invoke(update);
                });

                // Connection state change events
                _hubConnection.Closed += async (error) =>
                {
                    _logger.LogWarning("SignalR connection closed: {Error}", error?.Message);
                    OnConnectionStateChanged?.Invoke(false);
                    await Task.CompletedTask;
                };

                _hubConnection.Reconnecting += error =>
                {
                    _logger.LogInformation("SignalR reconnecting: {Error}", error?.Message);
                    OnConnectionStateChanged?.Invoke(false);
                    return Task.CompletedTask;
                };

                _hubConnection.Reconnected += connectionId =>
                {
                    _logger.LogInformation("SignalR reconnected: {ConnectionId}", connectionId);
                    OnConnectionStateChanged?.Invoke(true);
                    return Task.CompletedTask;
                };

                await _hubConnection.StartAsync();
                _logger.LogInformation("SignalR connection established");
                OnConnectionStateChanged?.Invoke(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to SignalR hub");
                OnConnectionStateChanged?.Invoke(false);
                throw;
            }
        }

        public async Task DisconnectAsync()
        {
            if (_hubConnection != null)
            {
                try
                {
                    await _hubConnection.StopAsync();
                    _logger.LogInformation("SignalR connection stopped");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error stopping SignalR connection");
                }
            }
        }

        public async Task JoinPortfolioGroupAsync(string portfolioId)
        {
            if (_hubConnection?.State == HubConnectionState.Connected && !string.IsNullOrEmpty(portfolioId))
            {
                try
                {
                    await _hubConnection.InvokeAsync("JoinPortfolioGroup", portfolioId);
                    _logger.LogInformation("Joined portfolio group: {PortfolioId}", portfolioId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error joining portfolio group: {PortfolioId}", portfolioId);
                    throw;
                }
            }
            else
            {
                throw new InvalidOperationException("Cannot join group: SignalR connection is not established");
            }
        }

        public async Task LeavePortfolioGroupAsync(string portfolioId)
        {
            if (_hubConnection?.State == HubConnectionState.Connected && !string.IsNullOrEmpty(portfolioId))
            {
                try
                {
                    await _hubConnection.InvokeAsync("LeavePortfolioGroup", portfolioId);
                    _logger.LogInformation("Left portfolio group: {PortfolioId}", portfolioId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error leaving portfolio group: {PortfolioId}", portfolioId);
                    throw;
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}
