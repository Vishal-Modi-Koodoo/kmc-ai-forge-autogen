using Microsoft.AspNetCore.Mvc;
using KMC_Forge_BTL_API.Contracts;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

namespace KMC_Forge_BTL_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SignalRTestController : ControllerBase
    {
        private readonly ISignalRService _signalRService;
        private readonly ILogger<SignalRTestController> _logger;

        public SignalRTestController(ISignalRService signalRService, ILogger<SignalRTestController> logger)
        {
            _signalRService = signalRService;
            _logger = logger;
        }

        [HttpPost("test-file-upload-status")]
        public async Task<IActionResult> TestFileUploadStatus([FromBody] TestSignalRRequest request)
        {
            try
            {
                await _signalRService.SendFileUploadStatusAsync(request.PortfolioId, request.Status, request.Message);
                return Ok(new { Message = "File upload status sent via SignalR", PortfolioId = request.PortfolioId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending file upload status via SignalR");
                return StatusCode(500, new { Error = "Failed to send SignalR message" });
            }
        }

        [HttpPost("test-validation-progress")]
        public async Task<IActionResult> TestValidationProgress([FromBody] TestValidationProgressRequest request)
        {
            try
            {
                await _signalRService.SendValidationProgressAsync(request.PortfolioId, request.CurrentStep, request.TotalSteps, request.Message);
                return Ok(new { Message = "Validation progress sent via SignalR", PortfolioId = request.PortfolioId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending validation progress via SignalR");
                return StatusCode(500, new { Error = "Failed to send SignalR message" });
            }
        }

        [HttpPost("test-validation-complete")]
        public async Task<IActionResult> TestValidationComplete([FromBody] TestValidationCompleteRequest request)
        {
            try
            {
                await _signalRService.SendValidationCompleteAsync(request.PortfolioId, request.Result);
                return Ok(new { Message = "Validation complete sent via SignalR", PortfolioId = request.PortfolioId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending validation complete via SignalR");
                return StatusCode(500, new { Error = "Failed to send SignalR message" });
            }
        }

        [HttpPost("test-validation-error")]
        public async Task<IActionResult> TestValidationError([FromBody] TestValidationErrorRequest request)
        {
            try
            {
                await _signalRService.SendValidationErrorAsync(request.PortfolioId, request.ErrorMessage);
                return Ok(new { Message = "Validation error sent via SignalR", PortfolioId = request.PortfolioId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending validation error via SignalR");
                return StatusCode(500, new { Error = "Failed to send SignalR message" });
            }
        }
    }

    public class TestSignalRRequest
    {
        public string PortfolioId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class TestValidationProgressRequest
    {
        public string PortfolioId { get; set; } = string.Empty;
        public int CurrentStep { get; set; }
        public int TotalSteps { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class TestValidationCompleteRequest
    {
        public string PortfolioId { get; set; } = string.Empty;
        public object Result { get; set; } = new();
    }

    public class TestValidationErrorRequest
    {
        public string PortfolioId { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
