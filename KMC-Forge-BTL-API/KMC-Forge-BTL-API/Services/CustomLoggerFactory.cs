using Microsoft.Extensions.Logging;

namespace KMC_AI_Forge_BTL_Agent.Services;

public class CustomLoggerFactory : ILoggerFactory
{
    private readonly IExternalScopeProvider? _scopeProvider;
    private bool _disposed;

    public CustomLoggerFactory(IExternalScopeProvider? scopeProvider = null)
    {
        _scopeProvider = scopeProvider;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new CustomLogger(categoryName, _scopeProvider);
    }

    public void AddProvider(ILoggerProvider provider)
    {
        // This method is called by the framework but we don't need to store providers
        // since we're using our custom logger implementation
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }
} 