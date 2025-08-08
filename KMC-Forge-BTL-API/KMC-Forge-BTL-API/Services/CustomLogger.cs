using Microsoft.Extensions.Logging;

namespace KMC_AI_Forge_BTL_Agent.Services;

public class CustomLogger : ILogger
{
    private readonly string _categoryName;
    private readonly IExternalScopeProvider? _scopeProvider;

    public CustomLogger(string categoryName, IExternalScopeProvider? scopeProvider = null)
    {
        _categoryName = categoryName;
        _scopeProvider = scopeProvider;
    }

    IDisposable ILogger.BeginScope<TState>(TState state) => _scopeProvider?.Push(state) ?? NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => true; // You can customize this based on your needs

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter?.Invoke(state, exception) ?? state?.ToString() ?? string.Empty;
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var logLevelString = logLevel.ToString().ToUpper();

        // Custom logging logic - you can extend this to write to files, databases, etc.
        var logEntry = $"[{timestamp}] [{logLevelString}] [{_categoryName}] {message}";
        
        if (exception != null)
        {
            logEntry += $"\nException: {exception.Message}\nStackTrace: {exception.StackTrace}";
        }

        // For now, just write to console - you can replace this with your preferred logging destination
        Console.WriteLine(logEntry);
        
        // You could also write to a file:
        // File.AppendAllText("logs/application.log", logEntry + Environment.NewLine);
    }

    private class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new NullScope();
        private NullScope() { }
        public void Dispose() { }
    }
} 